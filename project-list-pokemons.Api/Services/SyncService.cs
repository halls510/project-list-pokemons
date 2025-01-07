using project_list_pokemons.Api.Dtos;
using project_list_pokemons.Api.Interfaces.Repositories;
using project_list_pokemons.Api.Models;
using project_list_pokemons.Api.Utils;
using System.Diagnostics;
using System.Text.Json;

namespace project_list_pokemons.Api.Services
{
    public class SyncService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SyncService> _logger;
        private static bool _isSyncing = false;
        private static readonly object _lock = new();

        public SyncService(IServiceScopeFactory scopeFactory, ILogger<SyncService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            lock (_lock)
            {
                if (_isSyncing)
                {
                    _logger.LogWarning("Sincronização já em execução. Ignorando nova tentativa.");
                    return;
                }

                _isSyncing = true;
            }

            try
            {
                _logger.LogInformation("Iniciando sincronização inicial...");
                await SyncPokemons(stoppingToken);
                _logger.LogInformation("Sincronização inicial concluída com sucesso.");
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
                if (environment == "Production")
                {
                    _logger.LogInformation(@$"
                                        *********************************************
                                        *                                           *
                                        *        A API ESTÁ PRONTA PARA USO!        *
                                        *           Ambiente: Production            *
                                        *                                           *
                                        *********************************************
                                        ");
                }
                else
                {
                    _logger.LogInformation(@$"
                                        *********************************************
                                        *                                           *
                                        *        A API ESTÁ PRONTA PARA USO!        *
                                        *           Ambiente: Development           *
                                        *                                           *
                                        *********************************************
                                        ");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante a sincronização inicial.");
            }
            finally
            {
                _isSyncing = false;
            }

            // Define o valor padrão de 24 horas
            int defaultIntervalHours = 24;
            int intervalHours;           

            // Lê o intervalo das variáveis de ambiente (se existir) e tenta converter para inteiro
            if (!int.TryParse(Environment.GetEnvironmentVariable("SYNC_INTERVAL_HOURS"), out intervalHours))
            {
                intervalHours = defaultIntervalHours;
                _logger.LogInformation($"Variável de ambiente 'SYNC_INTERVAL_HOURS' não encontrada ou inválida. Usando valor padrão de {defaultIntervalHours} hora(s).");
            }
            else
            {
                _logger.LogInformation($"Intervalo de sincronização definido para {intervalHours} hora(s) a partir da variável de ambiente.");
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation($"Aguardando intervalo de {intervalHours} hora(s) para próxima sincronização com a PokeAPI...");
                    await Task.Delay(TimeSpan.FromHours(intervalHours), stoppingToken); 

                    _logger.LogInformation("Executando sincronização em segundo plano...");
                    await SyncPokemons(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro durante a execução da sincronização em segundo plano.");
                }
            }
        }

        private async Task SyncPokemons(CancellationToken stoppingToken)
        {
            var stopwatch = Stopwatch.StartNew(); // Inicia o cronômetro

            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IPokemonRepository>();
            var httpClient = scope.ServiceProvider.GetRequiredService<IHttpClientWrapper>();

            try
            {
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
                var responseapitotal = await httpClient.GetStringAsync("https://pokeapi.co/api/v2/pokemon?limit=0", stoppingToken);
                var pokemonApiResponseTotal = JsonSerializer.Deserialize<PokemonApiResponseTotal>(responseapitotal);
                int totalPokemons = pokemonApiResponseTotal?.Count ?? 0;

                // Configurar valores para desenvolvimento ou produção
                int batchSize = environment == "Development" ? 10 : 100; // Define tamanho do lote com base no ambiente
                totalPokemons = environment == "Development" ? Math.Min(totalPokemons, 50) : totalPokemons; // Limita o total de Pokémons em dev

                _logger.LogInformation("Iniciando sincronização com a PokéAPI. Total de Pokémons a processar: {totalPokemons}", totalPokemons);

                for (int offset = 0; offset < totalPokemons; offset += batchSize)
                {
                    stoppingToken.ThrowIfCancellationRequested(); // Interrompe se o token for cancelado

                    var rangeStart = offset + 1;
                    var rangeEnd = Math.Min(offset + batchSize, totalPokemons);

                    var response = await httpClient.GetStringAsync($"https://pokeapi.co/api/v2/pokemon?limit={batchSize}&offset={offset}", stoppingToken);
                    var data = JsonSerializer.Deserialize<PokemonApiResponse>(response);

                    if (data?.Results != null)
                    {
                        // Buscar todos os IDs existentes no banco
                        var existingIds = await repository.GetAllPokemonIdsAsync();
                        var idsInBatch = data.Results.Select(result => PokemonUtils.ExtractIdFromUrl(result.Url)).ToList();
                        var newIds = idsInBatch.Except(existingIds).ToList();
                        var updateIds = idsInBatch.Intersect(existingIds).ToList();

                        // Inserir novos Pokémons
                        if (newIds.Any())
                        {
                            var pokemonsToInsert = await FetchPokemonsByIds(httpClient, newIds, stoppingToken);
                            if (pokemonsToInsert.Any())
                            {
                                await repository.SavePokemonsAsync(pokemonsToInsert);
                                _logger.LogInformation("Sincronização em andamento: Inseridos {Count} Pokémons (Intervalo: {RangeStart} - {RangeEnd}).", pokemonsToInsert.Count, rangeStart, rangeEnd);
                            }
                        }

                        // Atualizar Pokémons existentes usando hash
                        if (updateIds.Any())
                        {
                            var existingPokemons = await repository.GetPokemonsByIdsAsync(updateIds);
                            var pokemonsFromApi = await FetchPokemonsByIds(httpClient, updateIds, stoppingToken);

                            var pokemonsToUpdate = new List<Pokemon>();

                            foreach (var apiPokemon in pokemonsFromApi)
                            {
                                var existingPokemon = existingPokemons.FirstOrDefault(p => p.Id == apiPokemon.Id);

                                if (existingPokemon != null)
                                {
                                    var newHash = HashUtils.GeneratePokemonHash(apiPokemon);

                                    // Atualiza somente se o hash mudou
                                    if (existingPokemon.Hash != newHash)
                                    {
                                        apiPokemon.Hash = newHash; // Atualiza o hash
                                        pokemonsToUpdate.Add(apiPokemon);
                                    }
                                }
                            }

                            if (pokemonsToUpdate.Any())
                            {
                                await repository.UpdatePokemonsAsync(pokemonsToUpdate);
                                _logger.LogInformation("Sincronização em andamento: Atualizados {Count} Pokémons (Intervalo: {RangeStart} - {RangeEnd}).", pokemonsToUpdate.Count, rangeStart, rangeEnd);
                            }
                        }
                    }
                }

                _logger.LogInformation("Sincronização concluída com sucesso.");
            }
            finally
            {
                stopwatch.Stop(); // Para o cronômetro
                _logger.LogInformation("Sincronização concluída em {ElapsedMilliseconds} ms.", stopwatch.ElapsedMilliseconds);
            }
        }

        private async Task<List<Pokemon>> FetchPokemonsByIds(IHttpClientWrapper httpClient, IEnumerable<int> ids, CancellationToken stoppingToken)
        {
            var tasks = ids.Select(async id =>
            {
                try
                {
                    var response = await httpClient.GetStringAsync($"https://pokeapi.co/api/v2/pokemon/{id}", stoppingToken);
                    var data = JsonSerializer.Deserialize<PokemonApiResponseItem>(response);

                    if (data != null)
                    {
                        var spriteUrl = data.Sprites.FrontDefault;
                        var base64Sprite = await PokemonUtils.ConvertImageToBase64(httpClient, spriteUrl, id, _logger);

                        // Obter evoluções     
                        var speciesResponse = await httpClient.GetStringAsync(data.Species.Url, stoppingToken);
                        var speciesData = JsonSerializer.Deserialize<PokemonSpeciesResponse>(speciesResponse);

                        var evolutions = new List<Evolution>();
                        if (speciesData?.EvolutionChain?.Url != null)
                        {
                            evolutions = await FetchEvolutions(httpClient, speciesData.EvolutionChain.Url, id, stoppingToken);
                        }

                        return new Pokemon
                        {
                            Id = id,
                            Name = data.Name,
                            Height = data.Height,
                            Weight = data.Weight,
                            BaseExperience = data.BaseExperience,
                            SpriteBase64 = base64Sprite,
                            Evolutions = evolutions,
                            Hash = HashUtils.GeneratePokemonHash(new Pokemon
                            {
                                Name = data.Name,
                                SpriteBase64 = base64Sprite,
                                Evolutions = evolutions
                            })
                        };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao buscar Pokémon ID: {Id}", id);
                }

                return null;
            });

            var results = await Task.WhenAll(tasks);
            return results.Where(pokemon => pokemon != null).ToList();
        }

        private async Task<List<Evolution>> FetchEvolutions(IHttpClientWrapper httpClient, string evolutionChainUrl, int pokemonId, CancellationToken stoppingToken)
        {
            try
            {
                var evolutionChainResponse = await httpClient.GetStringAsync(evolutionChainUrl, stoppingToken);
                var evolutionChain = JsonSerializer.Deserialize<EvolutionChainResponse>(evolutionChainResponse);

                var evolutions = new List<Evolution>();
                var currentChain = evolutionChain?.Chain;

                while (currentChain != null)
                {
                    try
                    {
                        var name = currentChain.Species.Name;                 

                        evolutions.Add(new Evolution
                        {
                            Name = name,
                            PokemonId = pokemonId
                        });

                        currentChain = currentChain.EvolvesTo.FirstOrDefault();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao processar evolução na cadeia para Pokémon ID: {PokemonId}", pokemonId);
                    }
                }

                return evolutions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter cadeia de evoluções para Pokémon ID: {PokemonId}", pokemonId);
                return new List<Evolution>();
            }
        }
       
    }
}

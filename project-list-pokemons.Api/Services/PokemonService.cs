using project_list_pokemons.Api.Dtos;
using project_list_pokemons.Api.Interfaces.Repositories;
using project_list_pokemons.Api.Interfaces.Services;
using project_list_pokemons.Api.Interfaces.Utils;
using project_list_pokemons.Api.Models;
using project_list_pokemons.Api.Parsers;
using project_list_pokemons.Api.Utils;
using System.Text.Json;

namespace project_list_pokemons.Api.Services
{
    public class PokemonService : IPokemonService
    {
        private readonly IPokemonRepository _repository;
        private readonly IRedisCacheHelper _cache;
        private readonly IHttpClientWrapper _httpClientWrapper;
        private readonly ILogger<PokemonService> _logger;

        public PokemonService(IPokemonRepository repository, IRedisCacheHelper cache, IHttpClientWrapper httpClientWrapper, ILogger<PokemonService> logger)
        {
            _repository = repository;
            _cache = cache;
            _httpClientWrapper = httpClientWrapper;
            _logger = logger;
        }

        /// <summary>
        /// Busca os dados de um Pokémon pelo ID.
        /// </summary>
        public async Task<PokemonResponse?> GetPokemonByIdAsync(int id)
        {
            string cacheKey = $"pokemon_{id}";

            try
            {
                // 1. Buscar no cache
                var cachedValue = await _cache.GetValueAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedValue))
                {
                    _logger.LogInformation("Cache HIT para Pokémon ID {Id}.", id);
                    return JsonSerializer.Deserialize<PokemonResponse>(cachedValue);
                }

                _logger.LogDebug("Cache MISS para Pokémon ID {Id}.", id);

                // 2. Buscar no banco de dados
                var pokemonFromDb = await _repository.GetPokemonByIdAsync(id);
                if (pokemonFromDb != null)
                {
                    var pokemonReponse = PokemonParser.ToDto(pokemonFromDb);

                    _logger.LogDebug("Pokémon ID {Id} encontrado no banco de dados.", id);

                    // Armazenar no cache
                    await _cache.SetValueAsync(cacheKey, JsonSerializer.Serialize(pokemonReponse), TimeSpan.FromMinutes(10));
                    return pokemonReponse;
                }

                // 3. Consultar na PokéAPI
                _logger.LogInformation("Pokémon ID {Id} não encontrado no banco. Consultando PokéAPI...", id);
                var pokemonFromApi = await FetchPokemonFromApiAsync(id);

                if (pokemonFromApi != null)
                {
                    var pokemonReponse = PokemonParser.ToDto(pokemonFromApi);

                    // Salvar no banco e no cache
                    await _repository.AddOrUpdatePokemonAsync(pokemonFromApi);
                    await _cache.SetValueAsync(cacheKey, JsonSerializer.Serialize(pokemonReponse), TimeSpan.FromMinutes(10));
                    return pokemonReponse;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar Pokémon ID {Id}.", id);
            }

            return null;
        }

        /// <summary>
        /// Lista os Pokémons cadastrados com paginação.
        /// </summary>
        /// <param name="page">Número da página (inicia em 1).</param>
        /// <param name="pageSize">Quantidade de registros por página.</param>
        /// <returns>Resultado paginado de Pokémons.</returns>
        public async Task<PaginacaoResultado<PokemonResponse>> ListarPokemonsComPaginacaoAsync(int page, int pageSize)
        {
            var totalItems = await _repository.CountPokemonsAsync();
            var pokemons = await _repository.GetPokemonsComPaginacaoAsync(page, pageSize);

            return new PaginacaoResultado<PokemonResponse>
            {
                TotalItens = totalItems,
                Itens = PokemonParser.ToDtoList(pokemons),
                PaginaAtual = page,
                TamanhoPagina = pageSize,
                TotalPaginas = (int)Math.Ceiling(totalItems / (double)pageSize)
            };
        }

        /// <summary>
        /// Consulta os dados de um Pokémon na PokéAPI.
        /// </summary>
        private async Task<Pokemon?> FetchPokemonFromApiAsync(int id)
        {
            try
            {
                var response = await _httpClientWrapper.GetStringAsync($"https://pokeapi.co/api/v2/pokemon/{id}");
                var data = JsonSerializer.Deserialize<PokemonApiResponseItem>(response);

                if (data != null)
                {
                    var spriteUrl = data.Sprites.FrontDefault;
                    var base64Sprite = await PokemonUtils.ConvertImageToBase64(_httpClientWrapper, spriteUrl,id, _logger);

                    return new Pokemon
                    {
                        Id = id,
                        Name = data.Name,
                        SpriteBase64 = base64Sprite
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar Pokémon ID {Id} na PokéAPI.", id);
            }

            return null;
        }

        public async Task<List<PokemonResponse>> GetRandomPokemonsAsync(int count)
        {
            var totalPokemons = await GetTotalPokemonsAsync();
            _logger.LogInformation("Buscando {Count} Pokémons aleatórios entre {TotalPokemons} disponíveis.", count, totalPokemons);
            var randomIds = Enumerable.Range(1, totalPokemons).OrderBy(x => Guid.NewGuid()).Take(count).ToList();

            var randomPokemons = new List<PokemonResponse>();
            foreach (var id in randomIds)
            {
                var pokemon = await GetPokemonByIdAsync(id);
                if (pokemon != null)
                {
                    randomPokemons.Add(pokemon);
                }
            }

            if (randomPokemons.Count == 0)
            {
                _logger.LogWarning("Não foi possível buscar Pokémons aleatórios. IDs sorteados: {RandomIds}.", string.Join(", ", randomIds));
                throw new InvalidOperationException("Não foi possível buscar Pokémons aleatórios.");
            }

            return randomPokemons;
        }

        public async Task<int> GetTotalPokemonsAsync()
        {
            string cacheKey = "total_pokemons";

            // Tenta buscar o total no cache
            var cachedValue = await _cache.GetValueAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedValue) && int.TryParse(cachedValue, out int cachedTotal))
            {
                return cachedTotal;
            }

            // Caso não esteja no cache, busca no banco
            var totalPokemons = await _repository.GetTotalPokemonsAsync();
            if (totalPokemons != null && totalPokemons > 0)
            {
                // Armazena no cache
                await _cache.SetValueAsync(cacheKey, totalPokemons.ToString(), TimeSpan.FromHours(24));
                return totalPokemons;
            }

            // Caso não esteja no banco, busca na PokéAPI
            try
            {
                var response = await _httpClientWrapper.GetStringAsync("https://pokeapi.co/api/v2/pokemon?limit=0");
                var apiResponse = JsonSerializer.Deserialize<PokemonApiResponseTotal>(response);

                if (apiResponse?.Count != null && apiResponse.Count > 0)
                {
                    // Armazena no banco e no cache
                    totalPokemons = apiResponse.Count.Value;                    
                    await _cache.SetValueAsync(cacheKey, totalPokemons.ToString(), TimeSpan.FromHours(24));

                    return totalPokemons;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar total de Pokémons na PokéAPI.");
            }

            // Valor padrão em caso de falha
            return 0;
        }

        /// <summary>
        /// Existe um Pokémon pelo ID.
        /// </summary>
        /// <param name="pokemonId">ID do Pokémon.</param>
        /// <returns>Retorna True ou False se não encontrado.</returns>
        public async Task<bool> ExistePokemonAsync(int pokemonId)
        {
            return await _repository.ExistsAsync(pokemonId);
        }

    }
}

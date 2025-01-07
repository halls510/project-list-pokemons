using project_list_pokemons.Api.Dtos;
using project_list_pokemons.Api.Interfaces.Repositories;
using project_list_pokemons.Api.Interfaces.Services;
using project_list_pokemons.Api.Interfaces.Utils;
using project_list_pokemons.Api.Parsers;
using project_list_pokemons.Api.Utils;
using System.Text.Json;

namespace project_list_pokemons.Api.Services
{
    public class MestrePokemonService : IMestrePokemonService
    {
        private readonly IMestrePokemonRepository _repository;
        private readonly IRedisCacheHelper _cache;
        private readonly ILogger<MestrePokemonService> _logger;

        public MestrePokemonService(IMestrePokemonRepository repository, IRedisCacheHelper cache, ILogger<MestrePokemonService> logger)
        {
            _repository = repository;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Cadastrar um Mestre Pokémon
        /// </summary>
        /// <param name="mestre"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<MestrePokemonResponse> CadastrarMestreAsync(MestrePokemonRequest mestre)
        {
            _logger.LogInformation("Cadastrando Mestre Pokémon: {Nome}", mestre.Nome);

            // Validação de CPF
            if (string.IsNullOrWhiteSpace(mestre.Cpf) || !CpfValidator.IsValid(mestre.Cpf))
            {
                throw new ArgumentException("O CPF informado é inválido.");
            }

            // Validação de Idade
            if (mestre.Idade <= 0)
            {
                _logger.LogWarning("Idade inválida fornecida para o Mestre Pokémon: {Idade}.", mestre.Idade);
                throw new ArgumentException("A idade informada é inválida. Informe uma idade maior ou igual a 1.");
            }

            // Verificar duplicidade de CPF
            if (await _repository.ExisteCpfAsync(mestre.Cpf))
            {
                throw new ArgumentException("O CPF informado já está cadastrado.");
            }

            // Tentar adicionar o mestre ao banco de dados
            var resultado = await _repository.AddAsync(MestrePokemonParser.ToEntity(mestre));

            // Verificar se o resultado é nulo
            if (resultado == null)
            {
                _logger.LogError("Erro ao cadastrar o Mestre Pokémon: {Nome}", mestre.Nome);
                throw new InvalidOperationException("Não foi possível cadastrar o Mestre Pokémon. Tente novamente.");
            }

            // Invalida o cache para garantir consistência
            await _cache.SetValueAsync($"mestrepokemon_{resultado.Id}", null, TimeSpan.Zero);

            _logger.LogInformation("Mestre Pokémon {Nome} cadastrado com sucesso.", mestre.Nome);
            return MestrePokemonParser.ToDto(resultado);
        }

        /// <summary>
        /// Busca um Mestre Pokémon por Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<MestrePokemonResponse?> BuscarMestrePorIdAsync(int id)
        {
            try
            {
                // Tenta buscar no cache
                var cacheKey = $"mestrepokemon_{id}";
                var cachedValue = await _cache.GetValueAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedValue))
                {
                    _logger.LogInformation("Cache HIT para Mestre Pokémon com ID {Id}.", id);
                    return JsonSerializer.Deserialize<MestrePokemonResponse>(cachedValue);
                }

                _logger.LogInformation("Cache MISS para Mestre Pokémon com ID {Id}.", id);

                // Se não encontrar no cache, consulta no banco
                var mestre = await _repository.GetByIdAsync(id);
                if (mestre != null)
                {
                    _logger.LogInformation("Mestre Pokémon com ID {Id} encontrado no banco de dados.", id);

                    var resultado = MestrePokemonParser.ToDto(mestre);

                    // Armazena no cache
                    await _cache.SetValueAsync(cacheKey, JsonSerializer.Serialize(resultado), TimeSpan.FromMinutes(10));

                    return resultado;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar Pokémon ID {Id}.", id);
            }

            return null;
        }

        /// <summary>
        /// Existe um Mestre pelo ID.
        /// </summary>
        /// <param name="mestrePokemonId">ID do Mestre.</param>
        /// <returns>Retorna True ou False se não encontrado.</returns>
        public async Task<bool> ExisteMestrePokemonAsync(int mestrePokemonId)
        {
            return await _repository.ExistsAsync(mestrePokemonId);
        }
    }
}

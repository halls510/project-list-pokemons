using project_list_pokemons.Api.Dtos;
using project_list_pokemons.Api.Exceptions.MestrePokemon;
using project_list_pokemons.Api.Interfaces.Repositories;
using project_list_pokemons.Api.Interfaces.Services;
using project_list_pokemons.Api.Parsers;

namespace project_list_pokemons.Api.Services
{
    public class CapturaService : ICapturaService
    {
        private readonly ICapturaRepository _capturaRepository;
        private readonly IMestrePokemonService _mestrePokemonService;
        private readonly IPokemonService _pokemonService;
        private readonly ILogger<MestrePokemonService> _logger;


        public CapturaService(ICapturaRepository capturaRepository, IMestrePokemonService mestrePokemonService, IPokemonService pokemonService, ILogger<MestrePokemonService> logger)
        {
            _capturaRepository = capturaRepository;
            _mestrePokemonService = mestrePokemonService;
            _pokemonService = pokemonService;
            _logger = logger;
        }

        /// <summary>
        /// Captura um Pokémon para o mestre.
        /// </summary>
        /// <param name="capturaRequest">Requisição contendo os detalhes da captura.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        /// <exception cref="InvalidOperationException">Lançado se o Pokémon já foi capturado ou se o mestre ou Pokémon não existir.</exception>
        public async Task CapturarPokemonAsync(CapturaRequest capturaRequest)
        {
            // Verificar se o mestre Pokémon existe
            var mestre = await _mestrePokemonService.ExisteMestrePokemonAsync(capturaRequest.MestrePokemonId);
            if (mestre == false)
            {
                _logger.LogWarning("Mestre Pokémon com ID {MestrePokemonId} não encontrado.", capturaRequest.MestrePokemonId);
                throw new InvalidOperationException("O mestre Pokémon não existe.");
            }

            // Verificar se o Pokémon existe
            var pokemon = await _pokemonService.ExistePokemonAsync(capturaRequest.PokemonId);
            if (pokemon == false)
            {
                _logger.LogWarning("Pokémon com ID {PokemonId} não encontrado.", capturaRequest.PokemonId);
                throw new InvalidOperationException("O Pokémon não existe.");
            }

            // Verificar se o Pokémon já foi capturado
            if (await _capturaRepository.IsPokemonAlreadyCapturedAsync(capturaRequest.MestrePokemonId, capturaRequest.PokemonId))
            {
                _logger.LogWarning("Este Pokémon ID {PokemonId} já foi capturado pelo mestre ID {MestrePokemonId}.", capturaRequest.PokemonId, capturaRequest.MestrePokemonId);
                throw new InvalidOperationException("Este Pokémon já foi capturado por este mestre.");
            }

            // Adicionar captura
            await _capturaRepository.AddCapturaAsync(CapturaParser.ToEntity(capturaRequest));
            _logger.LogInformation("Pokémon ID {PokemonId} capturado com sucesso pelo mestre ID {MestrePokemonId}.", capturaRequest.PokemonId, capturaRequest.MestrePokemonId);
        }


        /// <summary>
        /// Lista os Pokémons capturados por um mestre com paginação.
        /// </summary>
        /// <param name="mestrePokemonId">ID do mestre.</param>
        /// <param name="page">Número da página.</param>
        /// <param name="pageSize">Tamanho da página.</param>
        /// <returns>Resultado paginado de Pokémons capturados.</returns>
        public async Task<PaginacaoResultado<CapturaResponse>> ListarPokemonsCapturadosAsync(int mestrePokemonId, int page, int pageSize)
        {
            var mestre = await _mestrePokemonService.BuscarMestrePorIdAsync(mestrePokemonId);
            if (mestre == null)
            {
                _logger.LogWarning("Mestre Pokémon com ID {mestrePokemonId} não encontrado.", mestrePokemonId);
                throw new MestreNaoEncontradoException("Mestre Pokémon não encontrado.");
            }

            var totalItems = await _capturaRepository.CountCapturasByMestreAsync(mestrePokemonId);
            var pokemonsCapturados = await _capturaRepository.GetCapturasByMestreAsync(mestrePokemonId, page, pageSize);

            return new PaginacaoResultado<CapturaResponse>
            {
                TotalItens = totalItems,
                Itens = CapturaParser.ToDtoList(pokemonsCapturados),
                PaginaAtual = page,
                TamanhoPagina = pageSize,
                TotalPaginas = (int)Math.Ceiling(totalItems / (double)pageSize)
            };
        }

        /// <summary>
        /// Lista todos os Pokémons capturados com paginação.
        /// </summary>
        /// <param name="page">Número da página.</param>
        /// <param name="pageSize">Tamanho da página.</param>
        /// <returns>Resultado paginado de Pokémons capturados.</returns>
        public async Task<PaginacaoResultado<CapturaResponse>> ListarTodosPokemonsCapturadosAsync(int page, int pageSize)
        {
            var totalItems = await _capturaRepository.CountAllCapturasAsync();
            var pokemonsCapturados = await _capturaRepository.GetAllCapturasAsync(page, pageSize);

            return new PaginacaoResultado<CapturaResponse>
            {
                TotalItens = totalItems,
                Itens = CapturaParser.ToDtoList(pokemonsCapturados),
                PaginaAtual = page,
                TamanhoPagina = pageSize,
                TotalPaginas = (int)Math.Ceiling(totalItems / (double)pageSize)
            };
        }   

    }
}

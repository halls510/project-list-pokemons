using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project_list_pokemons.Api.Dtos;
using project_list_pokemons.Api.Interfaces.Services;

namespace project_list_pokemons.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PokemonController : ControllerBase
    {
        private readonly IPokemonService _pokemonService;
        private readonly ILogger<PokemonController> _logger;

        public PokemonController(IPokemonService pokemonService, ILogger<PokemonController> logger)
        {
            _pokemonService = pokemonService;
            _logger = logger;
        }

        /// <summary>
        /// Retorna 10 Pokémons aleatórios.
        /// </summary>
        /// <returns>Lista com 10 Pokémons aleatórios.</returns>
        /// <response code="200">Pokémons retornados com sucesso.</response>
        /// <response code="500">Erro interno do servidor.</response>
        [HttpGet("aleatorios")]
        public async Task<IActionResult> GetRandomPokemons()
        {
            try
            {
                var pokemons = await _pokemonService.GetRandomPokemonsAsync(10);
                return Ok(pokemons);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter Pokémons aleatórios.");
                return StatusCode(500, new ApiResponse(
                                message: "Erro interno do servidor.",
                                statusCode: "500_INTERNAL_SERVER_ERROR",
                                details: ex.Message
                            ));
            }
        }

        /// <summary>
        /// Retorna detalhes de um Pokémon pelo ID.
        /// </summary>
        /// <param name="id">ID do Pokémon.</param>
        /// <returns>Detalhes do Pokémon.</returns>
        /// <response code="200">Pokémon encontrado com sucesso.</response>
        /// <response code="404">Pokémon não encontrado.</response>
        /// <response code="500">Erro interno do servidor.</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPokemonById(int id)
        {
            try
            {
                var pokemon = await _pokemonService.GetPokemonByIdAsync(id);
                if (pokemon == null)
                {
                    _logger.LogWarning("Pokémon com ID {Id} não encontrado.", id);
                    return NotFound(new ApiResponse(
                                    message: "Pokémon não encontrado.",
                                    statusCode: "404_NOT_FOUND"
                                ));
                }

                return Ok(pokemon);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar Pokémon ID {Id}.", id);
                return StatusCode(500, new ApiResponse(
                                 message: "Erro interno do servidor.",
                                 statusCode: "500_INTERNAL_SERVER_ERROR",
                                 details: ex.Message
                             ));
            }
        }

        /// <summary>
        /// Retorna uma lista de Pokémons com paginação.
        /// </summary>
        /// <param name="page">Número da página (opcional, padrão = 1).</param>
        /// <param name="pageSize">Tamanho da página (opcional, padrão = 10).</param>
        /// <returns>Lista paginada de Pokémons.</returns>
        /// <response code="200">Pokémons retornados com sucesso.</response>
        /// <response code="500">Erro interno do servidor.</response>
        [HttpGet("todos")]
        public async Task<IActionResult> GetAllPokemons([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var resultado = await _pokemonService.ListarPokemonsComPaginacaoAsync(page, pageSize);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar Pokémons com paginação.");
                return StatusCode(500, new ApiResponse(
                                    message: "Erro interno do servidor.",
                                    statusCode: "500_INTERNAL_SERVER_ERROR",                                   
                                    details: ex.Message
                                ));
            }
        }

    }
}

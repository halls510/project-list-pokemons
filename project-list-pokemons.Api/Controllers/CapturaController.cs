using Microsoft.AspNetCore.Mvc;
using project_list_pokemons.Api.Dtos;
using project_list_pokemons.Api.Exceptions.Captura;
using project_list_pokemons.Api.Exceptions.MestrePokemon;
using project_list_pokemons.Api.Interfaces.Services;

namespace project_list_pokemons.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CapturaController : ControllerBase
    {
        private readonly ICapturaService _capturaService;
        private readonly ILogger<CapturaController> _logger;

        public CapturaController(ICapturaService capturaService, ILogger<CapturaController> logger)
        {
            _capturaService = capturaService;
            _logger = logger;
        }

        /// <summary>
        /// Marca um Pokémon como capturado.
        /// </summary>
        /// <param name="capturaRequest">Dados do mestre e do Pokémon a serem capturados.</param>
        /// <returns>Mensagem de sucesso ou erro.</returns>
        /// <response code="200">Pokémon capturado com sucesso.</response>
        /// <response code="400">Requisição inválida ou dados inconsistentes.</response>
        /// <response code="500">Erro interno do servidor.</response>
        [HttpPost("capturar")]
        public async Task<IActionResult> CapturarPokemon([FromBody] CapturaRequest capturaRequest)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Requisição inválida para capturar Pokémon: {ModelStateErrors}", ModelState.Values.SelectMany(v => v.Errors));
                return BadRequest(new ApiResponse(
                                      message: "Requisição inválida. Verifique os dados enviados.",
                                      statusCode: "400_BAD_REQUEST"
                                  ));
            }

            try
            {
                await _capturaService.CapturarPokemonAsync(capturaRequest);
                _logger.LogInformation("Pokémon ID {PokemonId} capturado com sucesso pelo mestre ID {MestrePokemonId}.", capturaRequest.PokemonId, capturaRequest.MestrePokemonId);
                return Ok(new ApiResponse(
                               message: "Pokémon capturado com sucesso.",
                               statusCode: "200_OK"
                           ));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Erro de validação ao capturar Pokémon: {Message}", ex.Message);
                return BadRequest(new ApiResponse(
                                   message: ex.Message,
                                   statusCode: "400_BAD_REQUEST"
                               ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao capturar Pokémon ID {PokemonId} para o mestre ID {MestrePokemonId}.", capturaRequest.PokemonId, capturaRequest.MestrePokemonId);
                return StatusCode(500, new ApiResponse(
                    message: "Erro interno do servidor.",
                    statusCode: "500_INTERNAL_SERVER_ERROR",
                    details: ex.Message
                ));
            }
        }


        /// <summary>
        /// Lista todos os Pokémons capturados com paginação.
        /// </summary>
        /// <param name="page">Número da página (opcional, padrão = 1).</param>
        /// <param name="pageSize">Tamanho da página (opcional, padrão = 10).</param>
        /// <returns>Lista de Pokémons capturados com paginação.</returns>
        /// <response code="200">Lista de Pokémons capturados retornada com sucesso.</response>
        /// <response code="500">Erro interno do servidor.</response>
        [HttpGet("todos")]
        public async Task<IActionResult> ListarTodosPokemonsCapturados([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var resultado = await _capturaService.ListarTodosPokemonsCapturadosAsync(page, pageSize);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar todos os Pokémons capturados.");
                return StatusCode(500, new ApiResponse(
                   message: "Erro interno do servidor.",
                   statusCode: "500_INTERNAL_SERVER_ERROR",
                   details: ex.Message
               ));
            }
        }

        /// <summary>
        /// Lista todos os Pokémons capturados por um mestre com paginação.
        /// </summary>
        /// <param name="mestrePokemonId">ID do mestre.</param>
        /// <param name="page">Número da página (opcional, padrão = 1).</param>
        /// <param name="pageSize">Tamanho da página (opcional, padrão = 10).</param>
        /// <returns>Lista de Pokémons capturados pelo mestre com paginação.</returns>
        /// <response code="200">Lista de Pokémons capturados pelo mestre retornada com sucesso.</response>
        /// <response code="404">Mestre não encontrado ou Nenhum Pokémon capturado pelo mestre.</response>
        /// <response code="500">Erro interno do servidor.</response>
        [HttpGet("mestre/{mestrePokemonId}")]
        public async Task<IActionResult> ListarPokemonsCapturadosPorMestre(int mestrePokemonId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var resultado = await _capturaService.ListarPokemonsCapturadosAsync(mestrePokemonId, page, pageSize);
                return Ok(resultado);
            }
            catch (MestreNaoEncontradoException ex)
            {
                _logger.LogWarning(ex, "Mestre não encontrado com ID {MestrePokemonId}.", mestrePokemonId);
                return NotFound(new ApiResponse(
                                      message: "Mestre Pokémon não encontrado.",
                                      statusCode: "404_NOT_FOUND"
                                  ));
            }
            catch (NenhumPokemonCapturadoException ex)
            {
                _logger.LogWarning(ex, "Nenhum Pokémon capturado pelo Mestre ID {MestrePokemonId}.", mestrePokemonId);
                return NotFound(new ApiResponse(
                       message: "Nenhum Pokémon capturado pelo Mestre.",
                       statusCode: "404_NOT_FOUND"
                   ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar Pokémons capturados pelo mestre ID {MestrePokemonId}.", mestrePokemonId);
                return StatusCode(500, new ApiResponse(
                   message: "Erro interno do servidor.",
                   statusCode: "500_INTERNAL_SERVER_ERROR",
                   details: ex.Message
               ));
            }
        }

    }

}

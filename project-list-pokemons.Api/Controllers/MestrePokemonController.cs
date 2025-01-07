using Microsoft.AspNetCore.Mvc;
using project_list_pokemons.Api.Dtos;
using project_list_pokemons.Api.Interfaces.Services;

namespace project_list_pokemons.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MestrePokemonController : ControllerBase
    {
        private readonly IMestrePokemonService _service;
        private readonly ILogger<MestrePokemonController> _logger;

        public MestrePokemonController(IMestrePokemonService service, ILogger<MestrePokemonController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Cadastra um novo Mestre Pokémon.
        /// </summary>
        /// <param name="mestre">Dados do Mestre Pokémon a ser cadastrado.</param>
        /// <returns>Dados do Mestre Pokémon cadastrado.</returns>
        /// <response code="201">Mestre Pokémon cadastrado com sucesso.</response>
        /// <response code="400">Dados inválidos ou erro de validação.</response>
        /// <response code="500">Erro interno do servidor.</response>
        [HttpPost]
        public async Task<IActionResult> CadastrarMestre([FromBody] MestrePokemonRequest request)
        {
            try
            {
                _logger.LogInformation("Recebendo solicitação para cadastrar Mestre Pokémon.");
                   
                var resultado = await _service.CadastrarMestreAsync(request);
                return CreatedAtAction(nameof(CadastrarMestre), new { id = resultado.Id }, resultado);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Erro de validação no cadastro.");
                return BadRequest(new ApiResponse(
                                    message: ex.Message,
                                    statusCode: "400_BAD_REQUEST"
                                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar solicitação de cadastro.");
                return StatusCode(500, new ApiResponse(
                                   message: "Erro interno do servidor.",
                                   statusCode: "500_INTERNAL_SERVER_ERROR",
                                   details: ex.Message
                               ));
            }
        }

        /// <summary>
        /// Busca um Mestre Pokémon pelo ID.
        /// </summary>
        /// <param name="id">ID do Mestre Pokémon.</param>
        /// <returns>Dados do Mestre Pokémon encontrado.</returns>
        /// <response code="200">Mestre Pokémon encontrado com sucesso.</response>
        /// <response code="404">Mestre Pokémon não encontrado.</response>
        /// <response code="500">Erro interno do servidor.</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> BuscarMestrePorId(int id)
        {
            try
            {
                var mestre = await _service.BuscarMestrePorIdAsync(id);

                if (mestre == null)
                {
                    _logger.LogWarning("Mestre Pokémon com ID {Id} não encontrado.", id);
                    return NotFound(new ApiResponse(
                                        message: "Mestre Pokémon não encontrado.",
                                        statusCode: "404_NOT_FOUND"
                                    ));
                }

                return Ok(mestre);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar Mestre Pokémon com ID {Id}.", id);
                return StatusCode(500, new ApiResponse(
                                     message: "Erro interno do servidor.",
                                     statusCode: "500_INTERNAL_SERVER_ERROR",
                                     details: ex.Message
                                 ));
            }
        }
    }
}

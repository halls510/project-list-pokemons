using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using project_list_pokemons.Api.Controllers;
using project_list_pokemons.Api.Dtos;
using project_list_pokemons.Api.Exceptions.Captura;
using project_list_pokemons.Api.Exceptions.MestrePokemon;
using project_list_pokemons.Api.Interfaces.Services;

namespace project_list_pokemons.Tests.Controllers
{
    public class CapturaControllerTests
    {
        private readonly Mock<ICapturaService> _capturaServiceMock;
        private readonly Mock<ILogger<CapturaController>> _loggerMock;
        private readonly CapturaController _controller;

        public CapturaControllerTests()
        {
            _capturaServiceMock = new Mock<ICapturaService>();
            _loggerMock = new Mock<ILogger<CapturaController>>();
            _controller = new CapturaController(_capturaServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task CapturarPokemon_RetornaOk_QuandoCapturaComSucesso()
        {
            // Arrange
            var request = new CapturaRequest { PokemonId = 1, MestrePokemonId = 100 };

            _capturaServiceMock
                .Setup(service => service.CapturarPokemonAsync(request))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.CapturarPokemon(request);

            // Assert
            // Verifica se a resposta é do tipo OkObjectResult
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Verifica o código de status retornado (200 - Ok)
            Assert.Equal(200, okResult.StatusCode);

            // Verifica que o conteúdo retornado é uma instância de ApiResponse
            var responseValue = Assert.IsType<ApiResponse>(okResult.Value);

            // Valida os valores dentro da resposta de sucesso
            Assert.Equal("Pokémon capturado com sucesso.", responseValue.Message);
            Assert.Equal("200_OK", responseValue.StatusCode);
            Assert.Null(responseValue.Details);

            // Este teste valida que, quando a captura é bem-sucedida, o controlador retorna
            // corretamente um status HTTP 200 com uma instância da classe ApiResponse.
        }

        [Fact]
        public async Task CapturarPokemon_RetornaBadRequest_QuandoDadosInvalidos()
        {
            // Arrange
            var request = new CapturaRequest { PokemonId = 1, MestrePokemonId = 100 };
            _capturaServiceMock
                .Setup(service => service.CapturarPokemonAsync(request))
                .ThrowsAsync(new InvalidOperationException("Dados inválidos."));

            // Act
            var result = await _controller.CapturarPokemon(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);

            var responseValue = Assert.IsType<ApiResponse>(badRequestResult.Value);
            Assert.Equal("Dados inválidos.", responseValue.Message);
            Assert.Equal("400_BAD_REQUEST", responseValue.StatusCode);
            Assert.Null(responseValue.Details);
        }

        [Fact]
        public async Task CapturarPokemon_RetornaErroServidor_QuandoExcecaoLancada()
        {
            // Arrange
            var request = new CapturaRequest { PokemonId = 1, MestrePokemonId = 100 };
            _capturaServiceMock
                .Setup(service => service.CapturarPokemonAsync(request))
                .ThrowsAsync(new Exception("Erro inesperado."));

            // Act
            var result = await _controller.CapturarPokemon(request);

            // Assert
            var serverErrorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, serverErrorResult.StatusCode);

            var responseValue = Assert.IsType<ApiResponse>(serverErrorResult.Value);
            Assert.Equal("Erro interno do servidor.", responseValue.Message);
            Assert.Equal("500_INTERNAL_SERVER_ERROR", responseValue.StatusCode);
            Assert.Equal("Erro inesperado.", responseValue.Details);
        }

        [Fact]
        public async Task ListarTodosPokemonsCapturados_RetornaOk_QuandoListaComSucesso()
        {
            // Arrange
            var paginatedCapturas = new PaginacaoResultado<CapturaResponse>
            {
                TotalItens = 15,
                PaginaAtual = 1,
                TamanhoPagina = 10,
                TotalPaginas = 2,
                Itens = new List<CapturaResponse>
                        {
                            new CapturaResponse
                            {
                                Id = 1,
                                MestrePokemonId = 100,
                                PokemonId = 1,
                                PokemonNome = "Pikachu",
                                DataCaptura = new DateTime(2025, 01, 01)
                            },
                            new CapturaResponse
                            {
                                Id = 2,
                                MestrePokemonId = 101,
                                PokemonId = 2,
                                PokemonNome = "Charmander",
                                DataCaptura = new DateTime(2025, 01, 02)
                            }
                        }
            };

            _capturaServiceMock
                .Setup(service => service.ListarTodosPokemonsCapturadosAsync(1, 10))
                .ReturnsAsync(paginatedCapturas);

            // Act
            var result = await _controller.ListarTodosPokemonsCapturados(1, 10);

            // Assert
            // Verifica se a resposta é do tipo OkObjectResult
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Verifica o código de status retornado (200 - OK)
            Assert.Equal(200, okResult.StatusCode);

            // Verifica que o valor retornado é do tipo PaginacaoResultado<CapturaResponse>
            var responseValue = Assert.IsType<PaginacaoResultado<CapturaResponse>>(okResult.Value);

            // Valida os dados retornados
            Assert.Equal(paginatedCapturas.TotalItens, responseValue.TotalItens);
            Assert.Equal(paginatedCapturas.PaginaAtual, responseValue.PaginaAtual);
            Assert.Equal(paginatedCapturas.TamanhoPagina, responseValue.TamanhoPagina);
            Assert.Equal(paginatedCapturas.TotalPaginas, responseValue.TotalPaginas);

            // Valida os itens capturados
            Assert.Equal(paginatedCapturas.Itens, responseValue.Itens);

            // Este teste valida que o controlador retorna corretamente uma lista paginada de capturas de Pokémons,
            // utilizando a estrutura PaginacaoResultado<T> e os dados de CapturaResponse.
        }


        [Fact]
        public async Task ListarTodosPokemonsCapturados_RetornaErroServidor_QuandoExcecaoLancada()
        {
            // Arrange
            _capturaServiceMock
                .Setup(service => service.ListarTodosPokemonsCapturadosAsync(1, 10))
                .ThrowsAsync(new Exception("Erro inesperado."));

            // Act
            var result = await _controller.ListarTodosPokemonsCapturados(1, 10);

            // Assert
            var serverErrorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, serverErrorResult.StatusCode);

            var responseValue = Assert.IsType<ApiResponse>(serverErrorResult.Value);
            Assert.Equal("Erro interno do servidor.", responseValue.Message);
            Assert.Equal("500_INTERNAL_SERVER_ERROR", responseValue.StatusCode);
            Assert.Equal("Erro inesperado.", responseValue.Details);
        }

        [Fact]
        public async Task ListarPokemonsCapturadosPorMestre_RetornaOk_QuandoListaComSucesso()
        {
            // Arrange
            var paginatedCapturas = new PaginacaoResultado<CapturaResponse>
            {
                TotalItens = 15,
                PaginaAtual = 1,
                TamanhoPagina = 10,
                TotalPaginas = 2,
                Itens = new List<CapturaResponse>
                     {
                         new CapturaResponse
                         {
                             Id = 1,
                             MestrePokemonId = 100,
                             PokemonId = 1,
                             PokemonNome = "Pikachu",
                             DataCaptura = new DateTime(2025, 01, 01)
                         },
                         new CapturaResponse
                         {
                             Id = 2,
                             MestrePokemonId = 101,
                             PokemonId = 2,
                             PokemonNome = "Charmander",
                             DataCaptura = new DateTime(2025, 01, 02)
                         }
                     }
            };
            _capturaServiceMock
                .Setup(service => service.ListarPokemonsCapturadosAsync(100, 1, 10))
                .ReturnsAsync(paginatedCapturas);

            // Act
            var result = await _controller.ListarPokemonsCapturadosPorMestre(100, 1, 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(paginatedCapturas, okResult.Value);
        }

        [Fact]
        public async Task ListarPokemonsCapturadosPorMestre_RetornaNotFound_QuandoMestrePokemonNaoEncontrado()
        {
            // Arrange
            _capturaServiceMock
                .Setup(service => service.ListarPokemonsCapturadosAsync(100, 1, 10))
                .ThrowsAsync(new MestreNaoEncontradoException("Mestre Pokémon não encontrado."));

            // Act
            var result = await _controller.ListarPokemonsCapturadosPorMestre(100, 1, 10);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);

            var responseValue = Assert.IsType<ApiResponse>(notFoundResult.Value);
            Assert.Equal("Mestre Pokémon não encontrado.", responseValue.Message);
            Assert.Equal("404_NOT_FOUND", responseValue.StatusCode);
            Assert.Null(responseValue.Details);
        }

        [Fact]
        public async Task ListarPokemonsCapturadosPorMestre_RetornaNotFound_QuandoNenhumPokemonEncontrado()
        {
            // Arrange
            _capturaServiceMock
                .Setup(service => service.ListarPokemonsCapturadosAsync(100, 1, 10))
                .ThrowsAsync(new NenhumPokemonCapturadoException("Nenhum Pokémon capturado pelo Mestre."));

            // Act
            var result = await _controller.ListarPokemonsCapturadosPorMestre(100, 1, 10);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);

            var responseValue = Assert.IsType<ApiResponse>(notFoundResult.Value);
            Assert.Equal("Nenhum Pokémon capturado pelo Mestre.", responseValue.Message);
            Assert.Equal("404_NOT_FOUND", responseValue.StatusCode);
            Assert.Null(responseValue.Details);
        }

        [Fact]
        public async Task ListarPokemonsCapturadosPorMestre_RetornaErroServidor_QuandoExcecaoLancada()
        {
            // Arrange
            _capturaServiceMock
                .Setup(service => service.ListarPokemonsCapturadosAsync(100, 1, 10))
                .ThrowsAsync(new Exception("Erro inesperado."));

            // Act
            var result = await _controller.ListarPokemonsCapturadosPorMestre(100, 1, 10);

            // Assert
            var serverErrorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, serverErrorResult.StatusCode);

            var responseValue = Assert.IsType<ApiResponse>(serverErrorResult.Value);
            Assert.Equal("Erro interno do servidor.", responseValue.Message);
            Assert.Equal("500_INTERNAL_SERVER_ERROR", responseValue.StatusCode);
            Assert.Equal("Erro inesperado.", responseValue.Details);
        }
    }
}

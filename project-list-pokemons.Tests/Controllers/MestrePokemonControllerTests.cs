using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using project_list_pokemons.Api.Controllers;
using project_list_pokemons.Api.Dtos;
using project_list_pokemons.Api.Interfaces.Services;

namespace project_list_pokemons.Tests.Controllers
{
    public class MestrePokemonControllerTests
    {
        private readonly Mock<IMestrePokemonService> _serviceMock;
        private readonly Mock<ILogger<MestrePokemonController>> _loggerMock;
        private readonly MestrePokemonController _controller;

        public MestrePokemonControllerTests()
        {
            _serviceMock = new Mock<IMestrePokemonService>();
            _loggerMock = new Mock<ILogger<MestrePokemonController>>();
            _controller = new MestrePokemonController(_serviceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task CadastrarMestre_RetornaCreated_QuandoSucesso()
        {
            // Arrange
            var request = new MestrePokemonRequest
            {
                Nome = "Ash Ketchum",
                Idade = 10,
                Cpf = "12345678900"
            };

            var response = new MestrePokemonResponse
            {
                Id = 1,
                Nome = "Ash Ketchum",
                Idade = 10,
                Cpf = "12345678900"
            };

            _serviceMock
                .Setup(service => service.CadastrarMestreAsync(request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CadastrarMestre(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal(response, createdResult.Value);
            Assert.Equal(nameof(_controller.CadastrarMestre), createdResult.ActionName);
            Assert.Equal(response.Id, createdResult.RouteValues["id"]);
        }

        [Fact]
        public async Task CadastrarMestre_RetornaBadRequest_QuandoDadosInvalidos()
        {
            // Arrange
            var request = new MestrePokemonRequest
            {
                Nome = "",
                Idade = -1,
                Cpf = "00000000000"
            };

            var exceptionMessage = "Os dados fornecidos são inválidos.";

            _serviceMock
                .Setup(service => service.CadastrarMestreAsync(request))
                .ThrowsAsync(new ArgumentException(exceptionMessage));

            // Act
            var result = await _controller.CadastrarMestre(request);

            // Assert
            // Verifica se a resposta é do tipo BadRequestObjectResult
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            // Verifica o código de status retornado (400)
            Assert.Equal(400, badRequestResult.StatusCode);

            // Verifica que o conteúdo retornado é uma instância de ApiErrorResponse
            var responseValue = Assert.IsType<ApiResponse>(badRequestResult.Value);

            // Valida os valores dentro da resposta de erro
            Assert.Equal("Os dados fornecidos são inválidos.", responseValue.Message);
            Assert.Equal("400_BAD_REQUEST", responseValue.StatusCode);
            Assert.Null(responseValue.Details);

            // Este teste garante que, quando os dados são inválidos, o controlador retorna corretamente
            // um status HTTP 400 com uma instância da classe ApiErrorResponse contendo a mensagem de erro.
        }

        [Fact]
        public async Task CadastrarMestre_RetornaErroServidor_QuandoExcecaoLancada()
        {
            // Arrange
            var request = new MestrePokemonRequest
            {
                Nome = "Ash Ketchum",
                Idade = 10,
                Cpf = "12345678900"
            };

            _serviceMock
                .Setup(service => service.CadastrarMestreAsync(request))
                .ThrowsAsync(new Exception("Erro inesperado."));

            // Act
            var result = await _controller.CadastrarMestre(request);

            // Assert
            // Verifica se a resposta é do tipo ObjectResult
            var serverErrorResult = Assert.IsType<ObjectResult>(result);

            // Verifica o código de status retornado (500 - Internal Server Error)
            Assert.Equal(500, serverErrorResult.StatusCode);

            // Verifica que o conteúdo retornado é uma instância de ApiResponse
            var responseValue = Assert.IsType<ApiResponse>(serverErrorResult.Value);

            // Valida os valores dentro da resposta de erro
            Assert.Equal("Erro interno do servidor.", responseValue.Message);
            Assert.Equal("500_INTERNAL_SERVER_ERROR", responseValue.StatusCode);
            Assert.Equal("Erro inesperado.", responseValue.Details);

            // Este teste valida que, quando ocorre uma exceção inesperada, o controlador retorna
            // um status HTTP 500 com uma instância da classe ApiResponse contendo os detalhes do erro.
        }

        [Fact]
        public async Task BuscarMestrePorId_RetornaOk_QuandoMestreEncontrado()
        {
            // Arrange
            var response = new MestrePokemonResponse
            {
                Id = 1,
                Nome = "Ash Ketchum",
                Idade = 10,
                Cpf = "12345678900"
            };

            _serviceMock
                .Setup(service => service.BuscarMestrePorIdAsync(1))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.BuscarMestrePorId(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task BuscarMestrePorId_RetornaNotFound_QuandoMestreNaoEncontrado()
        {
            // Simula um cenário onde o Mestre Pokémon não é encontrado
            MestrePokemonResponse? mestrePokemon = null;

            // Arrange
            _serviceMock
                .Setup(service => service.BuscarMestrePorIdAsync(99))
                .ReturnsAsync(mestrePokemon);

            // Act
            var result = await _controller.BuscarMestrePorId(99);

            // Assert
            // Verifica se a resposta é do tipo NotFoundObjectResult
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);

            // Verifica o código de status retornado (404 - Not Found)
            Assert.Equal(404, notFoundResult.StatusCode);

            // Verifica que o conteúdo retornado é uma instância de ApiResponse
            var responseValue = Assert.IsType<ApiResponse>(notFoundResult.Value);

            // Valida os valores dentro da resposta de erro
            Assert.Equal("Mestre Pokémon não encontrado.", responseValue.Message);
            Assert.Equal("404_NOT_FOUND", responseValue.StatusCode);
            Assert.Null(responseValue.Details);

            // Este teste valida que, quando o Mestre Pokémon não é encontrado, o controlador retorna
            // corretamente um status HTTP 404 com uma instância da classe ApiResponse.
        }

        [Fact]
        public async Task BuscarMestrePorId_RetornaErroServidor_QuandoExcecaoLancada()
        {
            // Arrange
            _serviceMock
                .Setup(service => service.BuscarMestrePorIdAsync(1))
                .ThrowsAsync(new Exception("Erro inesperado."));

            // Act
            var result = await _controller.BuscarMestrePorId(1);

            // Assert
            // Verifica se a resposta é do tipo ObjectResult
            var serverErrorResult = Assert.IsType<ObjectResult>(result);

            // Verifica o código de status retornado (500 - Internal Server Error)
            Assert.Equal(500, serverErrorResult.StatusCode);

            // Verifica que o conteúdo retornado é uma instância de ApiResponse
            var responseValue = Assert.IsType<ApiResponse>(serverErrorResult.Value);

            // Valida os valores dentro da resposta de erro
            Assert.Equal("Erro interno do servidor.", responseValue.Message);
            Assert.Equal("500_INTERNAL_SERVER_ERROR", responseValue.StatusCode);
            Assert.Equal("Erro inesperado.", responseValue.Details);

            // Este teste valida que, quando ocorre uma exceção inesperada, o controlador retorna
            // corretamente um status HTTP 500 com uma instância da classe ApiResponse contendo os detalhes do erro.
        }

    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using project_list_pokemons.Api.Controllers;
using project_list_pokemons.Api.Dtos;

namespace project_list_pokemons.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _configurationMock = new Mock<IConfiguration>();
            _controller = new AuthController(_configurationMock.Object);
        }

        [Fact]
        public void Login_RetornaOk_QuandoCredenciaisValidas()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "admin",
                Password = "123456789"
            };

            // Configura o mock do IConfiguration para retornar uma chave secreta simulada
            _configurationMock
                .Setup(config => config["Jwt:Key"])
                .Returns("pH2P6EojyGy5J1L8JibsmLCgZ0E1H1dVmUzNi/nxFSY=tet");

            _configurationMock
                .Setup(config => config["Jwt:Issuer"])
                .Returns("project_list_pokemons_tests");         

            // Act
            var result = _controller.Login(loginRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Verifica o código de status retornado (200 - Ok)
            Assert.Equal(200, okResult.StatusCode);

            // Verifica que o conteúdo retornado é uma instância de ApiResponseToken
            var responseValue = Assert.IsType<ApiResponseToken>(okResult.Value);

            // Valida os valores dentro da resposta de sucesso
            Assert.NotNull(responseValue.Token); // Garante que o token foi gerado
            Assert.Equal("200_OK", responseValue.StatusCode);
            Assert.Null(responseValue.Details);

            // Este teste valida que, ao passar credenciais válidas, o controlador retorna
            // corretamente um status HTTP 200 com uma instância da classe ApiResponseToken,
            // contendo um token válido e o status correto.
        }

        [Fact]
        public void Login_RetornaUnauthorized_QuandoCredenciaisInvalidas()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "user",
                Password = "wrongpassword"
            };

            // Act
            var result = _controller.Login(loginRequest);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(401, unauthorizedResult.StatusCode);

            var responseValue = Assert.IsType<ApiResponse>(unauthorizedResult.Value);
            Assert.Equal("Credenciais inválidas.", responseValue.Message);
            Assert.Equal("401_UNAUTHORIZED", responseValue.StatusCode);
            Assert.Null(responseValue.Details);
        }

    }
}

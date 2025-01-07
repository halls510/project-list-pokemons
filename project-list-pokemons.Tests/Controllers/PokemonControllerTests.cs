using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using project_list_pokemons.Api.Controllers;
using project_list_pokemons.Api.Dtos;
using project_list_pokemons.Api.Interfaces.Services;

namespace project_list_pokemons.Tests.Controllers
{
    public class PokemonControllerTests
    {
        private readonly Mock<IPokemonService> _pokemonServiceMock;
        private readonly Mock<ILogger<PokemonController>> _loggerMock;
        private readonly PokemonController _controller;

        public PokemonControllerTests()
        {
            _pokemonServiceMock = new Mock<IPokemonService>();
            _loggerMock = new Mock<ILogger<PokemonController>>();
            _controller = new PokemonController(_pokemonServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetRandomPokemons_RetornaOk_QuandoBemSucedido()
        {
            // Arrange
            // Lista com 10 Pokémons simulados da PokeAPI
            var pokemons = new List<PokemonResponse>
            {
                 new PokemonResponse { Id = 1, Name = "bulbasaur", Height = 7, Weight = 69, BaseExperience = 64, SpriteBase64 = "", Evolutions = new List<EvolutionResponse>() },
                 new PokemonResponse { Id = 4, Name = "charmander", Height = 6, Weight = 85, BaseExperience = 62, SpriteBase64 = "", Evolutions = new List<EvolutionResponse>() },
                 new PokemonResponse { Id = 7, Name = "squirtle", Height = 5, Weight = 90, BaseExperience = 63, SpriteBase64 = "", Evolutions = new List<EvolutionResponse>() },
                 new PokemonResponse { Id = 25, Name = "pikachu", Height = 4, Weight = 60, BaseExperience = 112, SpriteBase64 = "", Evolutions = new List<EvolutionResponse>() },
                 new PokemonResponse { Id = 39, Name = "jigglypuff", Height = 5, Weight = 55, BaseExperience = 95, SpriteBase64 = "", Evolutions = new List<EvolutionResponse>() },
                 new PokemonResponse { Id = 52, Name = "meowth", Height = 4, Weight = 42, BaseExperience = 58, SpriteBase64 = "", Evolutions = new List<EvolutionResponse>() },
                 new PokemonResponse { Id = 54, Name = "psyduck", Height = 8, Weight = 196, BaseExperience = 80, SpriteBase64 = "", Evolutions = new List<EvolutionResponse>() },
                 new PokemonResponse { Id = 66, Name = "machop", Height = 8, Weight = 195, BaseExperience = 75, SpriteBase64 = "", Evolutions = new List<EvolutionResponse>() },
                 new PokemonResponse { Id = 81, Name = "magnemite", Height = 3, Weight = 60, BaseExperience = 65, SpriteBase64 = "", Evolutions = new List<EvolutionResponse>() },
                 new PokemonResponse { Id = 94, Name = "gengar", Height = 15, Weight = 405, BaseExperience = 190, SpriteBase64 = "", Evolutions = new List<EvolutionResponse>() }
            };

            // Configurando o mock para retornar a lista simulada
            _pokemonServiceMock
                .Setup(service => service.GetRandomPokemonsAsync(10))
                .ReturnsAsync(pokemons);

            // Act
            // Chamada ao método do controlador
            var result = await _controller.GetRandomPokemons();

            // Assert
            // Verificando se o resultado é do tipo OkObjectResult
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Validando o código de status da resposta
            Assert.Equal(200, okResult.StatusCode);

            // Garantindo que o conteúdo retornado é a lista esperada
            Assert.Equal(pokemons, okResult.Value);

            // Este teste verifica que o método GetRandomPokemons retorna com sucesso uma lista de 10 Pokémons.
            // O mock simula o serviço retornando os dados, e o controlador devolve esses dados no formato correto.
        }

        [Fact]
        public async Task GetRandomPokemons_RetornaErroServidor_QuandoExceptionLancada()
        {
            // Arrange
            // Simula uma exceção ao chamar o método GetRandomPokemonsAsync no serviço
            _pokemonServiceMock
                .Setup(service => service.GetRandomPokemonsAsync(10))
                .Throws(new Exception("Erro inesperado."));

            // Act
            // Executa o método GetRandomPokemons do controlador
            var result = await _controller.GetRandomPokemons();

            // Assert
            // Verifica se o tipo da resposta é ObjectResult (indica erro no servidor)
            var serverErrorResult = Assert.IsType<ObjectResult>(result);

            // Verifica se o código de status retornado é 500 (Internal Server Error)
            Assert.Equal(500, serverErrorResult.StatusCode);

            // Garante que o conteúdo retornado é uma instância de ApiResponse
            var responseValue = Assert.IsType<ApiResponse>(serverErrorResult.Value);

            // Valida os valores dentro da resposta de erro
            Assert.Equal("Erro interno do servidor.", responseValue.Message);
            Assert.Equal("500_INTERNAL_SERVER_ERROR", responseValue.StatusCode);
            Assert.Equal("Erro inesperado.", responseValue.Details);

            // Este teste valida que, quando ocorre uma exceção inesperada no serviço,
            // o controlador retorna corretamente um status HTTP 500 com uma instância da classe ApiResponse.
        }


        [Fact]
        public async Task GetPokemonById_RetornaOk_QuandoPokemonEncontrado()
        {
            // Arrange
            var pokemon = new PokemonResponse { Id = 1, Name = "bulbasaur", Height = 7, Weight = 69, BaseExperience = 64, SpriteBase64 = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/1.png", Evolutions = new List<EvolutionResponse>() };
            _pokemonServiceMock
                .Setup(service => service.GetPokemonByIdAsync(1))
                .ReturnsAsync(pokemon);

            // Act
            var result = await _controller.GetPokemonById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(pokemon, okResult.Value);
        }

        [Fact]
        public async Task GetPokemonById_RetornaNotFound_QuandoPokemonNaoEncontrado()
        {
            // Arrange
            // Simula um cenário onde o Pokémon não é encontrado
            PokemonResponse? pokemon = null;

            _pokemonServiceMock
                .Setup(service => service.GetPokemonByIdAsync(99))
                .ReturnsAsync(pokemon);

            // Act
            // Executa o método do controlador
            var result = await _controller.GetPokemonById(99);

            // Assert
            // Verifica se o tipo da resposta é NotFoundObjectResult (indica que o recurso não foi encontrado)
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);

            // Verifica se o código de status retornado é 404 (Not Found)
            Assert.Equal(404, notFoundResult.StatusCode);

            // Valida que o conteúdo retornado é uma instância de ApiResponse
            var responseValue = Assert.IsType<ApiResponse>(notFoundResult.Value);

            // Valida os valores dentro da resposta de erro
            Assert.Equal("Pokémon não encontrado.", responseValue.Message);
            Assert.Equal("404_NOT_FOUND", responseValue.StatusCode);
            Assert.Null(responseValue.Details);

            // Este teste valida que, quando o Pokémon não é encontrado, o controlador retorna
            // corretamente um status HTTP 404 com uma instância da classe ApiResponse.
        }


        [Fact]
        public async Task GetPokemonById_RetornaErroServidor_QuandoExceptionLancada()
        {
            // Arrange
            // Configura o mock do serviço para lançar uma exceção ao buscar um Pokémon por ID
            _pokemonServiceMock
                .Setup(service => service.GetPokemonByIdAsync(1))
                .Throws(new Exception("Erro inesperado."));

            // Act
            // Executa o método do controlador
            var result = await _controller.GetPokemonById(1);

            // Assert
            // Verifica se o tipo da resposta é ObjectResult (indica erro no servidor)
            var serverErrorResult = Assert.IsType<ObjectResult>(result);

            // Verifica se o código de status retornado é 500 (Internal Server Error)
            Assert.Equal(500, serverErrorResult.StatusCode);

            // Verifica que o conteúdo da resposta é uma instância de ApiResponse
            var responseValue = Assert.IsType<ApiResponse>(serverErrorResult.Value);

            // Valida os valores dentro da resposta de erro
            Assert.Equal("Erro interno do servidor.", responseValue.Message);
            Assert.Equal("500_INTERNAL_SERVER_ERROR", responseValue.StatusCode);
            Assert.Equal("Erro inesperado.", responseValue.Details);

            // Este teste valida que, ao ocorrer uma exceção inesperada ao buscar um Pokémon por ID,
            // o controlador retorna corretamente um status HTTP 500 com uma instância da classe ApiResponse.
        }

        [Fact]
        public async Task GetAllPokemons_RetornaOk_QuandoBemSucedido()
        {
            // Arrange
            var paginatedPokemons = new PaginacaoResultado<PokemonResponse>
            {
                TotalItens = 20,
                PaginaAtual = 1,
                TamanhoPagina = 10,
                TotalPaginas = 2,
                Itens = new List<PokemonResponse>
                {
                    new PokemonResponse { Id = 1, Name = "bulbasaur", Height = 7, Weight = 69, BaseExperience = 64, SpriteBase64 = "", Evolutions = new List<EvolutionResponse>() },
                    new PokemonResponse { Id = 4, Name = "charmander", Height = 6, Weight = 85, BaseExperience = 62, SpriteBase64 = "", Evolutions = new List<EvolutionResponse>() }
                }
            };

            _pokemonServiceMock
                .Setup(service => service.ListarPokemonsComPaginacaoAsync(1, 10))
                .ReturnsAsync(paginatedPokemons);

            // Act
            var result = await _controller.GetAllPokemons(1, 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Verifica se o código de status HTTP é 200 (OK)
            Assert.Equal(200, okResult.StatusCode);

            // Valida que o valor retornado é igual ao esperado
            var responseValue = Assert.IsType<PaginacaoResultado<PokemonResponse>>(okResult.Value);
            Assert.Equal(paginatedPokemons.TotalItens, responseValue.TotalItens);
            Assert.Equal(paginatedPokemons.PaginaAtual, responseValue.PaginaAtual);
            Assert.Equal(paginatedPokemons.TamanhoPagina, responseValue.TamanhoPagina);
            Assert.Equal(paginatedPokemons.TotalPaginas, responseValue.TotalPaginas);
            Assert.Equal(paginatedPokemons.Itens, responseValue.Itens);

            // Este teste garante que o controlador retorna corretamente uma lista paginada de Pokémons,
            // usando a estrutura PaginacaoResultado<T> para organizar os dados.
        }

        [Fact]
        public async Task GetAllPokemons_RetornaErroServidor_QuandoExceptionLancada()
        {
            // Arrange
            // Configura o mock do serviço para lançar uma exceção ao listar Pokémons
            _pokemonServiceMock
                .Setup(service => service.ListarPokemonsComPaginacaoAsync(1, 10))
                .Throws(new Exception("Erro inesperado."));

            // Act
            // Executa o método do controlador
            var result = await _controller.GetAllPokemons(1, 10);

            // Assert
            // Verifica se o tipo da resposta é ObjectResult (indica erro no servidor)
            var serverErrorResult = Assert.IsType<ObjectResult>(result);

            // Garante que o código de status retornado é 500 (Internal Server Error)
            Assert.Equal(500, serverErrorResult.StatusCode);

            // Valida que o conteúdo retornado é uma instância de ApiResponse
            var responseValue = Assert.IsType<ApiResponse>(serverErrorResult.Value);

            // Verifica os valores dentro da resposta de erro
            Assert.Equal("Erro interno do servidor.", responseValue.Message);
            Assert.Equal("500_INTERNAL_SERVER_ERROR", responseValue.StatusCode);
            Assert.Equal("Erro inesperado.", responseValue.Details);

            // Este teste valida que o controlador retorna corretamente um status HTTP 500
            // com uma instância da classe ApiResponse contendo mensagem, código e detalhes do erro.
        }
    }
}

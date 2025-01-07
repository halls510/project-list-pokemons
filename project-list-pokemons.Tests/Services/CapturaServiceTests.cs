using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using project_list_pokemons.Api.Dtos;
using project_list_pokemons.Api.Exceptions.MestrePokemon;
using project_list_pokemons.Api.Interfaces.Repositories;
using project_list_pokemons.Api.Interfaces.Services;
using project_list_pokemons.Api.Models;
using project_list_pokemons.Api.Parsers;
using project_list_pokemons.Api.Services;

namespace project_list_pokemons.Tests.Services
{
    public class CapturaServiceTests
    {
        private readonly Mock<ICapturaRepository> _mockCapturaRepository;
        private readonly Mock<IMestrePokemonService> _mockMestrePokemonService;
        private readonly Mock<IPokemonService> _mockPokemonService;
        private readonly Mock<ILogger<MestrePokemonService>> _mockLogger;
        private readonly CapturaService _service;

        public CapturaServiceTests()
        {
            _mockCapturaRepository = new Mock<ICapturaRepository>();
            _mockMestrePokemonService = new Mock<IMestrePokemonService>();
            _mockPokemonService = new Mock<IPokemonService>();
            _mockLogger = new Mock<ILogger<MestrePokemonService>>();

            _service = new CapturaService(
                _mockCapturaRepository.Object,
                _mockMestrePokemonService.Object,
                _mockPokemonService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task CapturarPokemonAsync_DeveLancarExcecao_SeMestreNaoExiste()
        {
            // Arrange
            var request = new CapturaRequest { MestrePokemonId = 1, PokemonId = 25 };

            _mockMestrePokemonService.Setup(s => s.ExisteMestrePokemonAsync(request.MestrePokemonId)).ReturnsAsync(false);

            // Act
            var act = async () => await _service.CapturarPokemonAsync(request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("O mestre Pokémon não existe.");
            _mockMestrePokemonService.Verify(s => s.ExisteMestrePokemonAsync(request.MestrePokemonId), Times.Once);
            _mockPokemonService.Verify(s => s.ExistePokemonAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task CapturarPokemonAsync_DeveLancarExcecao_SePokemonNaoExiste()
        {
            // Arrange
            var request = new CapturaRequest { MestrePokemonId = 1, PokemonId = 25 };

            _mockMestrePokemonService.Setup(s => s.ExisteMestrePokemonAsync(request.MestrePokemonId)).ReturnsAsync(true);
            _mockPokemonService.Setup(s => s.ExistePokemonAsync(request.PokemonId)).ReturnsAsync(false);

            // Act
            var act = async () => await _service.CapturarPokemonAsync(request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("O Pokémon não existe.");
            _mockPokemonService.Verify(s => s.ExistePokemonAsync(request.PokemonId), Times.Once);
            _mockCapturaRepository.Verify(r => r.IsPokemonAlreadyCapturedAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task CapturarPokemonAsync_DeveLancarExcecao_SePokemonJaCapturado()
        {
            // Arrange
            var request = new CapturaRequest { MestrePokemonId = 1, PokemonId = 25 };

            _mockMestrePokemonService.Setup(s => s.ExisteMestrePokemonAsync(request.MestrePokemonId)).ReturnsAsync(true);
            _mockPokemonService.Setup(s => s.ExistePokemonAsync(request.PokemonId)).ReturnsAsync(true);
            _mockCapturaRepository.Setup(r => r.IsPokemonAlreadyCapturedAsync(request.MestrePokemonId, request.PokemonId)).ReturnsAsync(true);

            // Act
            var act = async () => await _service.CapturarPokemonAsync(request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Este Pokémon já foi capturado por este mestre.");
            _mockCapturaRepository.Verify(r => r.IsPokemonAlreadyCapturedAsync(request.MestrePokemonId, request.PokemonId), Times.Once);
            _mockCapturaRepository.Verify(r => r.AddCapturaAsync(It.IsAny<Captura>()), Times.Never);
        }

        [Fact]
        public async Task CapturarPokemonAsync_DeveCapturarPokemon_ComSucesso()
        {
            // Arrange
            var request = new CapturaRequest { MestrePokemonId = 1, PokemonId = 25 };

            _mockMestrePokemonService.Setup(s => s.ExisteMestrePokemonAsync(request.MestrePokemonId)).ReturnsAsync(true);
            _mockPokemonService.Setup(s => s.ExistePokemonAsync(request.PokemonId)).ReturnsAsync(true);
            _mockCapturaRepository.Setup(r => r.IsPokemonAlreadyCapturedAsync(request.MestrePokemonId, request.PokemonId)).ReturnsAsync(false);

            // Act
            await _service.CapturarPokemonAsync(request);

            // Assert
            _mockCapturaRepository.Verify(r => r.AddCapturaAsync(It.IsAny<Captura>()), Times.Once);
        }

        [Fact]
        public async Task ListarPokemonsCapturadosAsync_DeveLancarExcecao_SeMestreNaoExiste()
        {
            // Arrange
            var mestrePokemonId = 1;
            var page = 1;
            var pageSize = 10;

            _mockMestrePokemonService.Setup(s => s.BuscarMestrePorIdAsync(mestrePokemonId)).ReturnsAsync((MestrePokemonResponse)null);

            // Act
            var act = async () => await _service.ListarPokemonsCapturadosAsync(mestrePokemonId, page, pageSize);

            // Assert
            await act.Should().ThrowAsync<MestreNaoEncontradoException>().WithMessage("Mestre Pokémon não encontrado.");
            _mockMestrePokemonService.Verify(s => s.BuscarMestrePorIdAsync(mestrePokemonId), Times.Once);
        }

        [Fact]
        public async Task ListarPokemonsCapturadosAsync_DeveRetornarPokemonsCapturados()
        {
            // Arrange
            var mestrePokemonId = 1;
            var page = 1;
            var pageSize = 10;
            var totalItems = 3;

            var pokemonsCapturados = new List<Captura>
            {
                new Captura { Id = 1, PokemonId = 25, MestrePokemonId = mestrePokemonId, DataCaptura = DateTime.UtcNow, Pokemon = new Pokemon { Id = 25, Name = "Pikachu" } },
                new Captura { Id = 2, PokemonId = 4, MestrePokemonId = mestrePokemonId, DataCaptura = DateTime.UtcNow, Pokemon = new Pokemon { Id = 4, Name = "Charmander" } },
                new Captura { Id = 3, PokemonId = 7, MestrePokemonId = mestrePokemonId, DataCaptura = DateTime.UtcNow, Pokemon = new Pokemon { Id = 7, Name = "Squirtle" } }
            };

            var pokemonsCapturadosResponse = CapturaParser.ToDtoList(pokemonsCapturados);         

            _mockMestrePokemonService
                .Setup(s => s.BuscarMestrePorIdAsync(mestrePokemonId))
                .ReturnsAsync(new MestrePokemonResponse { Id = mestrePokemonId });

            _mockCapturaRepository
                .Setup(r => r.CountCapturasByMestreAsync(mestrePokemonId))
                .ReturnsAsync(totalItems);

            _mockCapturaRepository
                .Setup(r => r.GetCapturasByMestreAsync(mestrePokemonId, page, pageSize))
                .ReturnsAsync(pokemonsCapturados);

            // Act
            var result = await _service.ListarPokemonsCapturadosAsync(mestrePokemonId, page, pageSize);

            // Assert
            result.Should().NotBeNull();
            result.TotalItens.Should().Be(totalItems);
            result.Itens.Should().HaveCount(pokemonsCapturados.Count);
            result.PaginaAtual.Should().Be(page);
            result.TamanhoPagina.Should().Be(pageSize);
            result.TotalPaginas.Should().Be((int)Math.Ceiling(totalItems / (double)pageSize));
            result.Itens.Should().BeEquivalentTo(pokemonsCapturadosResponse);

            _mockMestrePokemonService.Verify(s => s.BuscarMestrePorIdAsync(mestrePokemonId), Times.Once);
            _mockCapturaRepository.Verify(r => r.CountCapturasByMestreAsync(mestrePokemonId), Times.Once);
            _mockCapturaRepository.Verify(r => r.GetCapturasByMestreAsync(mestrePokemonId, page, pageSize), Times.Once);
        }

    }
}

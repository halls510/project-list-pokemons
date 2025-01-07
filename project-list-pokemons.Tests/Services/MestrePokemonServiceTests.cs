using Moq;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using project_list_pokemons.Api.Interfaces.Repositories;
using project_list_pokemons.Api.Interfaces.Utils;
using project_list_pokemons.Api.Services;
using project_list_pokemons.Api.Dtos;
using System.Text.Json;
using project_list_pokemons.Api.Models;

namespace project_list_pokemons.Tests.Services
{
    public class MestrePokemonServiceTests
    {
        private readonly Mock<IMestrePokemonRepository> _mockRepository;
        private readonly Mock<IRedisCacheHelper> _mockCache;
        private readonly Mock<ILogger<MestrePokemonService>> _mockLogger;
        private readonly MestrePokemonService _service;

        public MestrePokemonServiceTests()
        {
            _mockRepository = new Mock<IMestrePokemonRepository>();
            _mockCache = new Mock<IRedisCacheHelper>();
            _mockLogger = new Mock<ILogger<MestrePokemonService>>();

            _service = new MestrePokemonService(_mockRepository.Object, _mockCache.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CadastrarMestreAsync_DeveCadastrarMestre_ComDadosValidos()
        {
            // Arrange
            var request = new MestrePokemonRequest
            {
                Nome = "Ash Ketchum",
                Cpf = "12345678909",
                Idade = 10
            };

            var entity = new MestrePokemon { Id = 1, Nome = request.Nome, Cpf = request.Cpf, Idade = request.Idade };

            _mockRepository.Setup(r => r.ExisteCpfAsync(request.Cpf)).ReturnsAsync(false);
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<MestrePokemon>())).ReturnsAsync(entity);

            // Act
            var response = await _service.CadastrarMestreAsync(request);

            // Assert
            response.Should().NotBeNull();
            response.Nome.Should().Be(request.Nome);

            _mockRepository.Verify(r => r.ExisteCpfAsync(request.Cpf), Times.Once);
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<MestrePokemon>()), Times.Once);
            _mockCache.Verify(c => c.SetValueAsync(It.IsAny<string>(), null, TimeSpan.Zero), Times.Once);
        }

        [Fact]
        public async Task CadastrarMestreAsync_DeveLancarExcecao_SeCpfInvalido()
        {
            // Arrange
            var request = new MestrePokemonRequest
            {
                Nome = "Ash Ketchum",
                Cpf = "123",
                Idade = 10
            };

            // Act
            var act = async () => await _service.CadastrarMestreAsync(request);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("O CPF informado é inválido.");
            _mockRepository.Verify(r => r.ExisteCpfAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CadastrarMestreAsync_DeveLancarExcecao_SeIdadeInvalida()
        {
            // Arrange
            var request = new MestrePokemonRequest
            {
                Nome = "Ash Ketchum",
                Cpf = "12345678909",
                Idade = 0 // Idade inválida
            };

            // Act
            var act = async () => await _service.CadastrarMestreAsync(request);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("A idade informada é inválida. Informe uma idade maior ou igual a 1.");
            _mockRepository.Verify(r => r.ExisteCpfAsync(It.IsAny<string>()), Times.Never);
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<MestrePokemon>()), Times.Never);
        }

        [Fact]
        public async Task CadastrarMestreAsync_DeveLancarExcecao_SeCpfDuplicado()
        {
            // Arrange
            var request = new MestrePokemonRequest
            {
                Nome = "Ash Ketchum",
                Cpf = "12345678909",
                Idade = 10
            };

            _mockRepository.Setup(r => r.ExisteCpfAsync(request.Cpf)).ReturnsAsync(true); // CPF já existe

            // Act
            var act = async () => await _service.CadastrarMestreAsync(request);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("O CPF informado já está cadastrado.");
            _mockRepository.Verify(r => r.ExisteCpfAsync(request.Cpf), Times.Once);
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<MestrePokemon>()), Times.Never);
        }

        [Fact]
        public async Task CadastrarMestreAsync_DeveLancarExcecao_SeErroAoCadastrarNoBanco()
        {
            // Arrange
            var request = new MestrePokemonRequest
            {
                Nome = "Ash Ketchum",
                Cpf = "12345678909",
                Idade = 10
            };
            MestrePokemon mestrePokemon = null;
            _mockRepository.Setup(r => r.ExisteCpfAsync(request.Cpf)).ReturnsAsync(false); // CPF não existe
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<MestrePokemon>())).ReturnsAsync(mestrePokemon); // Simula erro ao salvar

            // Act
            var act = async () => await _service.CadastrarMestreAsync(request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Não foi possível cadastrar o Mestre Pokémon. Tente novamente.");
            _mockRepository.Verify(r => r.ExisteCpfAsync(request.Cpf), Times.Once);
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<MestrePokemon>()), Times.Once);
        }

        [Fact]
        public async Task BuscarMestrePorIdAsync_DeveRetornarMestre_DoCache()
        {
            // Arrange
            var id = 1;
            var expectedResponse = new MestrePokemonResponse
            {
                Id = id,
                Nome = "Ash Ketchum",
                Cpf = "12345678909",
                Idade = 10
            };

            // Serializa o objeto esperado para simular o valor armazenado no cache
            var cachedValue = JsonSerializer.Serialize(expectedResponse);

            _mockCache.Setup(c => c.GetValueAsync($"mestrepokemon_{id}")).ReturnsAsync(cachedValue);

            // Act
            var response = await _service.BuscarMestrePorIdAsync(id);

            // Assert
            response.Should().NotBeNull();
            response.Id.Should().Be(expectedResponse.Id);
            response.Nome.Should().Be(expectedResponse.Nome);
            response.Cpf.Should().Be(expectedResponse.Cpf);
            response.Idade.Should().Be(expectedResponse.Idade);

            _mockCache.Verify(c => c.GetValueAsync($"mestrepokemon_{id}"), Times.Once);
        }

        [Fact]
        public async Task BuscarMestrePorIdAsync_DeveRetornarMestre_DoBancoDeDados_QuandoCacheMiss()
        {
            // Arrange
            var id = 1;
            var entity = new MestrePokemon
            {
                Id = id,
                Nome = "Ash Ketchum",
                Cpf = "12345678909",
                Idade = 10
            }; 
            string cache = null;
            _mockCache.Setup(c => c.GetValueAsync($"mestrepokemon_{id}")).ReturnsAsync(cache);
            _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);

            // Act
            var response = await _service.BuscarMestrePorIdAsync(id);

            // Assert
            response.Should().NotBeNull();
            response.Nome.Should().Be("Ash Ketchum");

            _mockCache.Verify(c => c.GetValueAsync($"mestrepokemon_{id}"), Times.Once);
            _mockCache.Verify(c => c.SetValueAsync($"mestrepokemon_{id}", It.IsAny<string>(), TimeSpan.FromMinutes(10)), Times.Once);
        }


        [Fact]
        public async Task ExisteMestrePokemonAsync_DeveRetornarTrue_SeMestreExiste()
        {
            // Arrange
            var id = 1;

            _mockRepository.Setup(r => r.ExistsAsync(id)).ReturnsAsync(true);

            // Act
            var result = await _service.ExisteMestrePokemonAsync(id);

            // Assert
            result.Should().BeTrue();
            _mockRepository.Verify(r => r.ExistsAsync(id), Times.Once);
        }
    }
}

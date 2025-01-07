using Microsoft.Extensions.Logging;
using Moq;
using project_list_pokemons.Api.Interfaces.Repositories;
using project_list_pokemons.Api.Interfaces.Utils;
using project_list_pokemons.Api.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using project_list_pokemons.Api.Dtos;
using project_list_pokemons.Api.Parsers;
using project_list_pokemons.Api.Models;
using project_list_pokemons.Api.Utils;

namespace project_list_pokemons.Tests.Services
{
    public class PokemonServiceTests
    {
        private readonly Mock<IPokemonRepository> _mockRepository;
        private readonly Mock<IRedisCacheHelper> _mockCache;
        private readonly Mock<IHttpClientWrapper> _mockHttpClientWrapper;
        private readonly Mock<ILogger<PokemonService>> _mockLogger;
        private readonly PokemonService _service;

        public PokemonServiceTests()
        {
            _mockRepository = new Mock<IPokemonRepository>();
            _mockCache = new Mock<IRedisCacheHelper>();
            _mockHttpClientWrapper = new Mock<IHttpClientWrapper>();
            _mockLogger = new Mock<ILogger<PokemonService>>();

            _service = new PokemonService(
                _mockRepository.Object,
                _mockCache.Object,
                _mockHttpClientWrapper.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task GetPokemonByIdAsync_DeveRetornarPokemon_DoCache()
        {
            // Arrange
            var id = 1;
            var expectedPokemon = new PokemonResponse
            {
                Id = id,
                Name = "Pikachu",
                Height = 4,
                Weight = 60,
                BaseExperience = 112,
                SpriteBase64 = "",
                Evolutions = new List<EvolutionResponse>()
            };

            var cachedValue = JsonSerializer.Serialize(expectedPokemon);

            _mockCache.Setup(c => c.GetValueAsync($"pokemon_{id}")).ReturnsAsync(cachedValue);

            // Act
            var result = await _service.GetPokemonByIdAsync(id);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedPokemon);
            _mockCache.Verify(c => c.GetValueAsync($"pokemon_{id}"), Times.Once);
            _mockRepository.Verify(r => r.GetPokemonByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetPokemonByIdAsync_DeveRetornarPokemon_DoBancoDeDados()
        {
            // Arrange
            var id = 1;
            var pokemonEntity = new Pokemon
            {
                Id = id,
                Name = "Charmander",
                Height = 6,
                Weight = 85,
                BaseExperience = 62
            };

            var expectedResponse = PokemonParser.ToDto(pokemonEntity);

            _mockCache.Setup(c => c.GetValueAsync($"pokemon_{id}")).ReturnsAsync((string)null);
            _mockRepository.Setup(r => r.GetPokemonByIdAsync(id)).ReturnsAsync(pokemonEntity);

            // Act
            var result = await _service.GetPokemonByIdAsync(id);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedResponse);
            _mockCache.Verify(c => c.GetValueAsync($"pokemon_{id}"), Times.Once);
            _mockRepository.Verify(r => r.GetPokemonByIdAsync(id), Times.Once);
        }

        [Fact]
        public async Task GetPokemonByIdAsync_DeveRetornarNull_SePokemonNaoExistirEmNenhumaFonte()
        {
            // Arrange
            var id = 4;

            _mockCache.Setup(c => c.GetValueAsync($"pokemon_{id}")).ReturnsAsync((string)null);
            _mockRepository.Setup(r => r.GetPokemonByIdAsync(id)).ReturnsAsync((Pokemon)null);
            _mockHttpClientWrapper.Setup(c => c.GetStringAsync(It.IsAny<string>())).ReturnsAsync((string)null);

            // Act
            var result = await _service.GetPokemonByIdAsync(id);

            // Assert
            result.Should().BeNull();
            _mockCache.Verify(c => c.GetValueAsync($"pokemon_{id}"), Times.Once);
            _mockRepository.Verify(r => r.GetPokemonByIdAsync(id), Times.Once);
            _mockHttpClientWrapper.Verify(c => c.GetStringAsync(It.IsAny<string>()), Times.Once);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using project_list_pokemons.Api.Data;
using project_list_pokemons.Api.Models;
using project_list_pokemons.Api.Repositories;
using project_list_pokemons.Api.Services;

namespace project_list_pokemons.Tests.Repositories
{
    public class PokemonRepositoryTests : IAsyncLifetime
    {
        private readonly AppDbContext _dbContext;
        private readonly PokemonRepository _repository;

        public PokemonRepositoryTests()
        {
            // Configuração do banco de dados SQLite em memória
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("Filename=:memory:") // Banco SQLite em memória
                .Options;

            _dbContext = new AppDbContext(options);
            _dbContext.Database.OpenConnection(); // Abre a conexão
            _dbContext.Database.EnsureCreated(); // Cria o esquema do banco

            _repository = new PokemonRepository(_dbContext, Mock.Of<ILogger<PokemonService>>());
        }

        public async Task InitializeAsync()
        {
            await PreencherBancoDeDadosAsync();
        }

        public Task DisposeAsync() => Task.CompletedTask;

        private async Task PreencherBancoDeDadosAsync()
        {
            // Limpa os dados existentes
            _dbContext.Capturas.RemoveRange(_dbContext.Capturas);
            _dbContext.MestresPokemon.RemoveRange(_dbContext.MestresPokemon);
            _dbContext.Pokemons.RemoveRange(_dbContext.Pokemons);
            await _dbContext.SaveChangesAsync();

            // Preenche o banco com Pokémons
            var pokemons = new List<Pokemon>
                        {
                            new Pokemon { Id = 1, Name = "Bulbasaur", Height = 7, Weight = 69, BaseExperience = 64 },
                            new Pokemon { Id = 2, Name = "Ivysaur", Height = 10, Weight = 130, BaseExperience = 142 },
                            new Pokemon { Id = 3, Name = "Venusaur", Height = 20, Weight = 1000, BaseExperience = 236 }
                        };
            _dbContext.Pokemons.AddRange(pokemons);
            await _dbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task AddOrUpdatePokemonAsync_DeveAdicionarOuAtualizarPokemon()
        {
            // Arrange
            var novoPokemon = new Pokemon { Id = 4, Name = "Charmander", Height = 6, Weight = 85, BaseExperience = 62 };

            // Act
            await _repository.AddOrUpdatePokemonAsync(novoPokemon);

            // Assert
            var savedPokemon = await _dbContext.Pokemons.FirstOrDefaultAsync(p => p.Id == novoPokemon.Id);
            Assert.NotNull(savedPokemon);
            Assert.Equal(novoPokemon.Name, savedPokemon?.Name);
        }

        [Fact]
        public async Task GetPokemonByIdAsync_DeveRetornarPokemonPorId()
        {
            // Act
            var pokemon = await _repository.GetPokemonByIdAsync(1);

            // Assert
            Assert.NotNull(pokemon);
            Assert.Equal("Bulbasaur", pokemon?.Name);
        }

        [Fact]
        public async Task GetPokemonByIdAsync_DeveRetornarNull_SePokemonNaoExistir()
        {
            // Act
            var pokemon = await _repository.GetPokemonByIdAsync(99);

            // Assert
            Assert.Null(pokemon);
        }

        [Fact]
        public async Task GetAllPokemonIdsAsync_DeveRetornarListaDeIds()
        {
            // Act
            var ids = await _repository.GetAllPokemonIdsAsync();

            // Assert
            Assert.NotNull(ids);
            Assert.Equal(3, ids.Count);
            Assert.Contains(1, ids);
            Assert.Contains(2, ids);
            Assert.Contains(3, ids);
        }

        [Fact]
        public async Task GetPokemonsComPaginacaoAsync_DeveRetornarPokemonsPaginados()
        {
            // Act
            var pokemons = await _repository.GetPokemonsComPaginacaoAsync(1, 2);

            // Assert
            Assert.NotNull(pokemons);
            Assert.Equal(2, pokemons.Count);
            Assert.Equal("Bulbasaur", pokemons[0].Name);
            Assert.Equal("Ivysaur", pokemons[1].Name);
        }

        [Fact]
        public async Task ExistsAsync_DeveRetornarTrue_SePokemonExistir()
        {
            // Act
            var exists = await _repository.ExistsAsync(1);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_DeveRetornarFalse_SePokemonNaoExistir()
        {
            // Act
            var exists = await _repository.ExistsAsync(99);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task CountPokemonsAsync_DeveRetornarQuantidadeDePokemons()
        {
            // Act
            var count = await _repository.CountPokemonsAsync();

            // Assert
            Assert.Equal(3, count);
        }
    }

}

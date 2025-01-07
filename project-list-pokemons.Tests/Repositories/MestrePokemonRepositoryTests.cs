using Microsoft.EntityFrameworkCore;
using project_list_pokemons.Api.Data;
using project_list_pokemons.Api.Models;
using project_list_pokemons.Api.Repositories;

namespace project_list_pokemons.Tests.Repositories
{
    public class MestrePokemonRepositoryTests : IAsyncLifetime
    {
        private readonly AppDbContext _dbContext;
        private readonly MestrePokemonRepository _repository;

        public MestrePokemonRepositoryTests()
        {
            // Configuração do banco de dados SQLite em memória
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("Filename=:memory:") // Banco SQLite em memória
                .Options;

            _dbContext = new AppDbContext(options);
            _dbContext.Database.OpenConnection(); // Abre a conexão
            _dbContext.Database.EnsureCreated(); // Cria o esquema do banco

            _repository = new MestrePokemonRepository(_dbContext);
        }

        public async Task InitializeAsync()
        {
            await PreencherBancoDeDadosAsync();
        }

        public Task DisposeAsync() => Task.CompletedTask;

        private async Task PreencherBancoDeDadosAsync()
        {
            // Limpa os dados existentes na ordem correta para evitar conflitos de chave estrangeira
            _dbContext.Capturas.RemoveRange(_dbContext.Capturas);
            _dbContext.MestresPokemon.RemoveRange(_dbContext.MestresPokemon);
            await _dbContext.SaveChangesAsync();

            // Preenche o banco com Mestres Pokémon
            var mestres = new List<MestrePokemon>
        {
            new MestrePokemon { Id = 1, Nome = "Ash", Idade = 10, Cpf = "12345678901" },
            new MestrePokemon { Id = 2, Nome = "Misty", Idade = 12, Cpf = "09876543210" }
        };
            _dbContext.MestresPokemon.AddRange(mestres);
            await _dbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task ExistsAsync_DeveRetornarTrue_SeMestreExistir()
        {
            // Act
            var exists = await _repository.ExistsAsync(1);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_DeveRetornarFalse_SeMestreNaoExistir()
        {
            // Act
            var exists = await _repository.ExistsAsync(99);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task AddAsync_DeveAdicionarMestreNoBancoDeDados()
        {
            // Arrange
            var novoMestre = new MestrePokemon { Nome = "Brock", Cpf = "45678912300", Idade = 15 };

            // Act
            var result = await _repository.AddAsync(novoMestre);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(novoMestre.Nome, result.Nome);

            var savedMestre = await _dbContext.MestresPokemon.FirstOrDefaultAsync(m => m.Cpf == novoMestre.Cpf);
            Assert.NotNull(savedMestre);
            Assert.Equal("Brock", savedMestre?.Nome);
        }

        [Fact]
        public async Task GetByIdAsync_DeveRetornarMestrePorId()
        {
            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Ash", result?.Nome);
        }

        [Fact]
        public async Task GetByIdAsync_DeveRetornarNull_SeMestreNaoExistir()
        {
            // Act
            var result = await _repository.GetByIdAsync(99);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ExisteCpfAsync_DeveRetornarTrue_SeCpfExistir()
        {
            // Act
            var exists = await _repository.ExisteCpfAsync("12345678901");

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExisteCpfAsync_DeveRetornarFalse_SeCpfNaoExistir()
        {
            // Act
            var exists = await _repository.ExisteCpfAsync("00000000000");

            // Assert
            Assert.False(exists);
        }
    }


}

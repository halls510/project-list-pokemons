using Microsoft.EntityFrameworkCore;
using project_list_pokemons.Api.Data;
using project_list_pokemons.Api.Models;
using project_list_pokemons.Api.Repositories;

namespace project_list_pokemons.Tests.Repositories
{
    public class CapturaRepositoryTests : IAsyncLifetime
    {
        private readonly AppDbContext _dbContext;
        private readonly CapturaRepository _repository;

        public CapturaRepositoryTests()
        {
            // Configuração do banco de dados SQLite em memória
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("Filename=:memory:") // Banco SQLite em memória
                .Options;

            _dbContext = new AppDbContext(options);
            _dbContext.Database.OpenConnection(); // Abre a conexão
            _dbContext.Database.EnsureCreated(); // Cria o esquema do banco

            _repository = new CapturaRepository(_dbContext);
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
            _dbContext.Pokemons.RemoveRange(_dbContext.Pokemons);
            await _dbContext.SaveChangesAsync();

            // Preenche o banco com Pokémons
            var pokemons = new List<Pokemon>
                {
                    new Pokemon { Id = 25, Name = "Pikachu", Height = 4, Weight = 60, BaseExperience = 112 },
                    new Pokemon { Id = 4, Name = "Charmander", Height = 6, Weight = 85, BaseExperience = 62 },
                    new Pokemon { Id = 7, Name = "Squirtle", Height = 5, Weight = 90, BaseExperience = 63 }
                };
            _dbContext.Pokemons.AddRange(pokemons);
            await _dbContext.SaveChangesAsync();

            // Preenche o banco com Mestres Pokémon
            var mestres = new List<MestrePokemon>
            {
                new MestrePokemon { Id = 1, Nome = "Ash", Idade = 10, Cpf = "12345678901" },
                new MestrePokemon { Id = 2, Nome = "Misty", Idade = 12, Cpf = "09876543210" }
            };
            _dbContext.MestresPokemon.AddRange(mestres);
            await _dbContext.SaveChangesAsync();

            // Preenche o banco com Capturas
            var capturas = new List<Captura>
            {
                new Captura { MestrePokemonId = 1, PokemonId = 25, DataCaptura = DateTime.Now },
                new Captura { MestrePokemonId = 1, PokemonId = 4, DataCaptura = DateTime.Now },
                new Captura { MestrePokemonId = 2, PokemonId = 7, DataCaptura = DateTime.Now }
            };
            _dbContext.Capturas.AddRange(capturas);
            await _dbContext.SaveChangesAsync();
        }


        [Fact]
        public async Task AddCapturaAsync_DeveSalvarCapturaNoBanco()
        {
            // Arrange
            var novaCaptura = new Captura
            {
                MestrePokemonId = 2,
                PokemonId = 25,
                DataCaptura = DateTime.Now
            };

            // Act
            await _repository.AddCapturaAsync(novaCaptura);

            // Assert
            var savedCaptura = await _dbContext.Capturas.FirstOrDefaultAsync(c =>
                c.MestrePokemonId == novaCaptura.MestrePokemonId && c.PokemonId == novaCaptura.PokemonId);

            Assert.NotNull(savedCaptura);
            Assert.Equal(novaCaptura.PokemonId, savedCaptura?.PokemonId);
        }

        [Fact]
        public async Task GetCapturasByMestreAsync_DeveRetornarCapturasDoMestre()
        {
            // Act
            var capturas = await _repository.GetCapturasByMestreAsync(1, 1, 10);

            // Assert
            Assert.NotNull(capturas);
            Assert.Equal(2, capturas.Count);
            Assert.All(capturas, c => Assert.Equal(1, c.MestrePokemonId));
        }

        [Fact]
        public async Task CountAllCapturasAsync_DeveRetornarQuantidadeTotalDeCapturas()
        {

            // Act
            var totalCapturas = await _repository.CountAllCapturasAsync();

            // Assert
            Assert.Equal(3, totalCapturas);
        }

        [Fact]
        public async Task IsPokemonAlreadyCapturedAsync_DeveRetornarTrue_SePokemonCapturado()
        {
            // Act
            var alreadyCaptured = await _repository.IsPokemonAlreadyCapturedAsync(1, 25);

            // Assert
            Assert.True(alreadyCaptured);
        }

        [Fact]
        public async Task IsPokemonAlreadyCapturedAsync_DeveRetornarFalse_SePokemonNaoCapturado()
        {
            // Act
            var notCaptured = await _repository.IsPokemonAlreadyCapturedAsync(1, 99);

            // Assert
            Assert.False(notCaptured);
        }
    }
}

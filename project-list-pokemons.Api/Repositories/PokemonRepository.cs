using project_list_pokemons.Api.Data;
using project_list_pokemons.Api.Models;
using Microsoft.EntityFrameworkCore;
using project_list_pokemons.Api.Services;
using project_list_pokemons.Api.Interfaces.Repositories;

namespace project_list_pokemons.Api.Repositories
{
    public class PokemonRepository : IPokemonRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PokemonService> _logger;

        public PokemonRepository(AppDbContext context, ILogger<PokemonService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Retorna uma lista de Pokémons Paginados.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Pokemon>> GetPokemonsComPaginacaoAsync(int page, int pageSize)
        {
            return await GetPokemonsComPaginacaoInternalAsync(page, pageSize);
        }

        private async Task<List<Pokemon>> GetPokemonsComPaginacaoInternalAsync(int page, int pageSize)
        {
            return await _context.Pokemons
                .Include(m => m.Evolutions)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Count Total dos Pokémons.
        /// </summary>
        /// <returns></returns>
        public async Task<int> CountPokemonsAsync()
        {
            return await CountPokemonsInternalAsync();
        }

        private async Task<int> CountPokemonsInternalAsync()
        {
            return await _context.Pokemons.CountAsync();
        }


        /// <summary>
        /// Verifica se um Pokémon já existe no banco de dados.
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            return await ExistsInternalAsync(id);
        }

        /// <summary>
        /// Busca um Pokémon por ID no banco de dados.
        /// </summary>
        public async Task<Pokemon?> GetPokemonByIdAsync(int id)
        {
            return await GetPokemonByIdInternalAsync(id);
        }


        public async Task<List<Pokemon>> GetPokemonsByIdsAsync(IEnumerable<int> ids)
        {
            return await GetPokemonsByIdsInternalAsync(ids);
        }

        /// <summary>
        /// Adiciona ou atualiza um Pokémon no banco de dados.
        /// </summary>
        public async Task AddOrUpdatePokemonAsync(Pokemon pokemon)
        {
            await AddOrUpdatePokemonInternalAsync(pokemon);
        }

        /// <summary>
        /// Salva uma lista de Pokémons no banco de dados em uma transação.
        /// </summary>
        public async Task SavePokemonsAsync(IEnumerable<Pokemon> pokemons)
        {
            await SavePokemonsInternalAsync(pokemons);
        }

        /// <summary>
        /// Obtém o total de Pokémons armazenados no banco de dados.
        /// </summary>
        public async Task<int> GetTotalPokemonsAsync()
        {
            return await GetTotalPokemonsInternalAsync();
        }

        /// <summary>
        /// Retorna uma lista de Ids de todos os Pokémons.
        /// </summary>
        /// <returns></returns>
        public async Task<List<int>> GetAllPokemonIdsAsync()
        {
            return await GetAllPokemonIdsInternalAsync();
        }

        /// <summary>
        /// Atualiza uma lista de Pokémons no banco de dados em uma transação.
        /// </summary>
        public async Task UpdatePokemonsAsync(IEnumerable<Pokemon> pokemons)
        {
            await UpdatePokemonsInternalAsync(pokemons);
        }

        private async Task<bool> ExistsInternalAsync(int id)
        {
            return await _context.Pokemons.AnyAsync(p => p.Id == id);
        }

        private async Task<Pokemon?> GetPokemonByIdInternalAsync(int id)
        {
            var pokemon = await _context.Pokemons
                .Include(m => m.Evolutions)
                .FirstOrDefaultAsync(p => p.Id == id);

            return pokemon;
        }

        private async Task<List<Pokemon>> GetPokemonsByIdsInternalAsync(IEnumerable<int> ids)
        {
            var pokemons = await _context.Pokemons
                .Include(p => p.Evolutions)
                .Where(p => ids.Contains(p.Id))
                .ToListAsync();

            return pokemons;
        }

        private async Task UpdatePokemonsInternalAsync(IEnumerable<Pokemon> pokemons)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Detach todas as entidades do tipo Pokemon que estão rastreadas
                var trackedPokemons = _context.ChangeTracker.Entries<Pokemon>().ToList();
                foreach (var tracked in trackedPokemons)
                {
                    tracked.State = EntityState.Detached;
                }

                // Atualizar a lista de pokémons
                _context.UpdateRange(pokemons);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task AddOrUpdatePokemonInternalAsync(Pokemon pokemon)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var pokemonDb = _context.Pokemons
                     .Where(p => p.Id == pokemon.Id)
                     .FirstOrDefault();

                if (pokemonDb != null)
                {
                    var trackedPokemon = _context.ChangeTracker.Entries<Pokemon>()
                                         .FirstOrDefault(entry => entry.Entity.Id == pokemon.Id);

                    // Verifique se o Pokémon foi encontrado
                    if (trackedPokemon != null)
                    {
                        // Desanexar a entidade
                        trackedPokemon.State = EntityState.Detached;
                    }

                    // Atualiza Pokémon
                    _context.Update(pokemon);
                }
                else
                {
                    // Adiciona novo Pokémon
                    _context.Add(pokemon);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task SavePokemonsInternalAsync(IEnumerable<Pokemon> pokemons)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Adiciona novos Pokémon
                _context.AddRange(pokemons);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<int> GetTotalPokemonsInternalAsync()
        {
            return await _context.Pokemons.CountAsync();
        }

        private async Task<List<int>> GetAllPokemonIdsInternalAsync()
        {
            return await _context.Pokemons.Select(p => p.Id).ToListAsync();
        }
    }
}

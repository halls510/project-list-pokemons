using Microsoft.EntityFrameworkCore;
using project_list_pokemons.Api.Data;
using project_list_pokemons.Api.Interfaces.Repositories;
using project_list_pokemons.Api.Models;

namespace project_list_pokemons.Api.Repositories
{
    public class MestrePokemonRepository : IMestrePokemonRepository
    {
        private readonly AppDbContext _context;

        public MestrePokemonRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Verifica se um Mestre já existe no banco de dados.
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            return await ExistsInternalAsync(id);
        }

        /// <summary>
        /// Salva um Mestre Pokémon.
        /// </summary>
        /// <param name="mestre"></param>
        /// <returns></returns>
        public async Task<MestrePokemon> AddAsync(MestrePokemon mestre)
        {           
            return await AddInternalAsync(mestre);
        }

        /// <summary>
        /// Busca um Mestre Pokémon por ID no banco de dados.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<MestrePokemon?> GetByIdAsync(int id)
        {
            return await GetByIdInternalAsync(id);
        }

        /// <summary>
        /// Verifica se existe o Cpf de Mestre Pokémon no banco de dados.
        /// </summary>
        /// <param name="cpf"></param>
        /// <returns></returns>
        public async Task<bool> ExisteCpfAsync(string cpf)
        {
            return await ExisteCpfInternalAsync(cpf);
        }

        private async Task<bool> ExistsInternalAsync(int id)
        {
            return await _context.MestresPokemon.AnyAsync(p => p.Id == id);
        }

        private async Task<MestrePokemon> AddInternalAsync(MestrePokemon mestre)
        {
            // Inicia uma transação no contexto
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Adiciona o mestre ao contexto
                _context.Add(mestre);

                // Salva as alterações no banco de dados
                await _context.SaveChangesAsync();

                // Confirma a transação
                await transaction.CommitAsync();

                return mestre; // Retorna o mestre adicionado
            }
            catch (Exception)
            {
                // Reverte a transação em caso de erro
                await transaction.RollbackAsync();
                throw; // Repassa a exceção para o chamador
            }
        }

        private async Task<MestrePokemon?> GetByIdInternalAsync(int id)
        {
            return await _context.MestresPokemon
                .Include(m => m.Capturas) 
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        private async Task<bool> ExisteCpfInternalAsync(string cpf)
        {
            return await _context.MestresPokemon.AnyAsync(m => m.Cpf == cpf);
        }
    }
}

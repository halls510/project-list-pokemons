using Microsoft.EntityFrameworkCore;
using project_list_pokemons.Api.Data;
using project_list_pokemons.Api.Interfaces.Repositories;
using project_list_pokemons.Api.Models;

namespace project_list_pokemons.Api.Repositories
{
    public class CapturaRepository : ICapturaRepository
    {
        private readonly AppDbContext _context;

        public CapturaRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Salva uma Captura de Pokémon no banco de dados.
        /// </summary>
        /// <param name="captura"></param>
        /// <returns></returns>
        public async Task AddCapturaAsync(Captura captura)
        {
            await AddCapturaInternalAsync(captura);
        }

        /// <summary>
        /// Retorna uma lista paginada de Capturas de Pokémons por ID do Mestre Pokémon.
        /// </summary>
        /// <param name="mestrePokemonId">ID do Mestre Pokémon.</param>
        /// <param name="page">Número da página.</param>
        /// <param name="pageSize">Tamanho da página.</param>
        /// <returns>Lista paginada de capturas.</returns>
        public async Task<List<Captura>> GetCapturasByMestreAsync(int mestrePokemonId, int page, int pageSize)
        {
            return await GetCapturasByMestreInternalAsync(mestrePokemonId, page, pageSize);
        }

        private async Task<List<Captura>> GetCapturasByMestreInternalAsync(int mestrePokemonId, int page, int pageSize)
        {
            return await _context.Capturas
                .Include(c => c.Pokemon)
                .Include(c => c.MestrePokemon)
                .Where(c => c.MestrePokemonId == mestrePokemonId)
                .OrderBy(o => o.DataCaptura)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Retorna uma lista paginada de todas as Capturas de Pokémons.
        /// </summary>
        /// <param name="page">Número da página.</param>
        /// <param name="pageSize">Tamanho da página.</param>
        /// <returns>Lista paginada de capturas.</returns>
        public async Task<List<Captura>> GetAllCapturasAsync(int page, int pageSize)
        {
            return await GetAllCapturasInternalAsync(page, pageSize);
        }

        private async Task<List<Captura>> GetAllCapturasInternalAsync(int page, int pageSize)
        {
            return await _context.Capturas
                .Include(c => c.Pokemon)
                .Include(c => c.MestrePokemon)
                .OrderBy(o => o.DataCaptura)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Conta o número total de capturas de Pokémons.
        /// </summary>
        /// <returns>Número total de capturas.</returns>
        public async Task<int> CountAllCapturasAsync()
        {
            return await CountAllCapturasInternalAsync();
        }

        private async Task<int> CountAllCapturasInternalAsync()
        {
            return await _context.Capturas.CountAsync();
        }

        /// <summary>
        /// Conta o número total de capturas de Pokémons por Mestre.
        /// </summary>
        /// <param name="mestrePokemonId">ID do Mestre Pokémon.</param>
        /// <returns>Número total de capturas.</returns>
        public async Task<int> CountCapturasByMestreAsync(int mestrePokemonId)
        {
            return await CountCapturasByMestreInternalAsync(mestrePokemonId);
        }

        private async Task<int> CountCapturasByMestreInternalAsync(int mestrePokemonId)
        {
            return await _context.Capturas
                .Where(c => c.MestrePokemonId == mestrePokemonId)
                .CountAsync();
        }

        /// <summary>
        /// Verifica se o Pokémon já foi capturado.
        /// </summary>
        /// <param name="mestrePokemonId"></param>
        /// <param name="pokemonId"></param>
        /// <returns></returns>
        public async Task<bool> IsPokemonAlreadyCapturedAsync(int mestrePokemonId, int pokemonId)
        {
            return await IsPokemonAlreadyCapturedInternalAsync(mestrePokemonId, pokemonId);
        }

        private async Task AddCapturaInternalAsync(Captura captura)
        {
            // Inicia uma transação no contexto
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Adiciona a captura ao contexto
                await _context.Capturas.AddAsync(captura);

                // Salva as alterações no banco de dados
                await _context.SaveChangesAsync();

                // Confirma a transação
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                // Reverte a transação em caso de erro
                await transaction.RollbackAsync();
                throw; // Repassa a exceção para o chamador
            }
        }

        private async Task<bool> IsPokemonAlreadyCapturedInternalAsync(int mestrePokemonId, int pokemonId)
        {
            return await _context.Capturas.AnyAsync(c =>
                c.MestrePokemonId == mestrePokemonId &&
                c.PokemonId == pokemonId);
        }
    }
}

using project_list_pokemons.Api.Models;

namespace project_list_pokemons.Api.Interfaces.Repositories
{
    public interface IMestrePokemonRepository
    {
        /// <summary>
        /// Verifica se um Mestre já existe no banco de dados.
        /// </summary>
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// Salva um Mestre Pokémon.
        /// </summary>
        /// <param name="mestre"></param>
        /// <returns></returns>
        Task<MestrePokemon> AddAsync(MestrePokemon mestre);

        /// <summary>
        /// Busca um Mestre Pokémon por ID no banco de dados.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<MestrePokemon?> GetByIdAsync(int id);

        /// <summary>
        /// Verifica se existe o Cpf de Mestre Pokémon no banco de dados.
        /// </summary>
        /// <param name="cpf"></param>
        /// <returns></returns
        Task<bool> ExisteCpfAsync(string cpf);
    }
}

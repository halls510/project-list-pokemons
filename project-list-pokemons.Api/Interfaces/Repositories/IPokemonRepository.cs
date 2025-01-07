using project_list_pokemons.Api.Models;

namespace project_list_pokemons.Api.Interfaces.Repositories
{
    public interface IPokemonRepository
    {
        /// <summary>
        /// Retorna uma lista de Pokémons Paginados.
        /// </summary>
        Task<List<Pokemon>> GetPokemonsComPaginacaoAsync(int page, int pageSize);

        /// <summary>
        /// Count Total dos Pokémons.
        /// </summary>
        Task<int> CountPokemonsAsync();

        /// <summary>
        /// Verifica se um Pokémon já existe no banco de dados.
        /// </summary>
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// Busca um Pokémon por ID no banco de dados.
        /// </summary>
        Task<Pokemon?> GetPokemonByIdAsync(int id);

        /// <summary>
        /// Retorna uma lista de Pokémons por IDs.
        /// </summary>
        Task<List<Pokemon>> GetPokemonsByIdsAsync(IEnumerable<int> ids);

        /// <summary>
        /// Adiciona ou atualiza um Pokémon no banco de dados.
        /// </summary>
        Task AddOrUpdatePokemonAsync(Pokemon pokemon);

        /// <summary>
        /// Salva uma lista de Pokémons no banco de dados em uma transação.
        /// </summary>
        Task SavePokemonsAsync(IEnumerable<Pokemon> pokemons);

        /// <summary>
        /// Obtém o total de Pokémons armazenados no banco de dados.
        /// </summary>
        Task<int> GetTotalPokemonsAsync();

        /// <summary>
        /// Retorna uma lista de Ids de todos os Pokémons.
        /// </summary>
        Task<List<int>> GetAllPokemonIdsAsync();

        /// <summary>
        /// Atualiza uma lista de Pokémons no banco de dados em uma transação.
        /// </summary>
        Task UpdatePokemonsAsync(IEnumerable<Pokemon> pokemons);
    }
}

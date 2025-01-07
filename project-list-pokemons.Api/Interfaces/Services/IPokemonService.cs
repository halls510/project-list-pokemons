using project_list_pokemons.Api.Dtos;

namespace project_list_pokemons.Api.Interfaces.Services
{
    public interface IPokemonService
    {
        /// <summary>
        /// Busca os dados de um Pokémon pelo ID.
        /// </summary>
        /// <param name="id">ID do Pokémon.</param>
        /// <returns>Dados do Pokémon ou null se não encontrado.</returns>
        Task<PokemonResponse?> GetPokemonByIdAsync(int id);

        /// <summary>
        /// Lista os Pokémons cadastrados com paginação.
        /// </summary>
        /// <param name="page">Número da página (inicia em 1).</param>
        /// <param name="pageSize">Quantidade de registros por página.</param>
        /// <returns>Resultado paginado de Pokémons.</returns>
        Task<PaginacaoResultado<PokemonResponse>> ListarPokemonsComPaginacaoAsync(int page, int pageSize);

        /// <summary>
        /// Obtém uma lista de Pokémons aleatórios.
        /// </summary>
        /// <param name="count">Quantidade de Pokémons aleatórios a buscar.</param>
        /// <returns>Lista de Pokémons aleatórios.</returns>
        Task<List<PokemonResponse>> GetRandomPokemonsAsync(int count);

        /// <summary>
        /// Obtém o número total de Pokémons disponíveis.
        /// </summary>
        /// <returns>Número total de Pokémons.</returns>
        Task<int> GetTotalPokemonsAsync();

        /// <summary>
        /// Verifica se existe um Pokémon com o ID fornecido.
        /// </summary>
        /// <param name="pokemonId">ID do Pokémon.</param>
        /// <returns>True se o Pokémon existe; caso contrário, False.</returns>
        Task<bool> ExistePokemonAsync(int pokemonId);
    }
}

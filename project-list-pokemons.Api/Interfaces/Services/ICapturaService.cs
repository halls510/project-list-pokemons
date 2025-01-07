using project_list_pokemons.Api.Dtos;

namespace project_list_pokemons.Api.Interfaces.Services
{
    public interface ICapturaService
    {
        /// <summary>
        /// Captura um Pokémon para o mestre.
        /// </summary>
        /// <param name="capturaRequest">Requisição contendo os detalhes da captura.</param>
        /// <returns>Task representando a operação assíncrona.</returns>
        /// <exception cref="InvalidOperationException">Lançado se o Pokémon já foi capturado ou se o mestre ou Pokémon não existir.</exception>
        Task CapturarPokemonAsync(CapturaRequest capturaRequest);

        /// <summary>
        /// Lista os Pokémons capturados por um mestre com paginação.
        /// </summary>
        /// <param name="mestrePokemonId">ID do mestre.</param>
        /// <param name="page">Número da página.</param>
        /// <param name="pageSize">Tamanho da página.</param>
        /// <returns>Resultado paginado de Pokémons capturados.</returns>
        Task<PaginacaoResultado<CapturaResponse>> ListarPokemonsCapturadosAsync(int mestrePokemonId, int page, int pageSize);

        /// <summary>
        /// Lista todos os Pokémons capturados com paginação.
        /// </summary>
        /// <param name="page">Número da página.</param>
        /// <param name="pageSize">Tamanho da página.</param>
        /// <returns>Resultado paginado de Pokémons capturados.</returns>
        Task<PaginacaoResultado<CapturaResponse>> ListarTodosPokemonsCapturadosAsync(int page, int pageSize);
    }
}

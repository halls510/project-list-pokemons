using project_list_pokemons.Api.Models;

namespace project_list_pokemons.Api.Interfaces.Repositories
{
    public interface ICapturaRepository
    {
        /// <summary>
        /// Salva uma Captura de Pokémon no banco de dados.
        /// </summary>
        /// <param name="captura"></param>
        /// <returns></returns>
        Task AddCapturaAsync(Captura captura);

        /// <summary>
        /// Retorna uma lista paginada de Capturas de Pokémons por ID do Mestre Pokémon.
        /// </summary>
        /// <param name="mestrePokemonId">ID do Mestre Pokémon.</param>
        /// <param name="page">Número da página.</param>
        /// <param name="pageSize">Tamanho da página.</param>
        /// <returns>Lista paginada de capturas.</returns>
        Task<List<Captura>> GetCapturasByMestreAsync(int mestrePokemonId, int page, int pageSize);

        /// <summary>
        /// Retorna uma lista paginada de todas as Capturas de Pokémons.
        /// </summary>
        /// <param name="page">Número da página.</param>
        /// <param name="pageSize">Tamanho da página.</param>
        /// <returns>Lista paginada de capturas.</returns>
        Task<List<Captura>> GetAllCapturasAsync(int page, int pageSize);

        /// <summary>
        /// Conta o número total de capturas de Pokémons.
        /// </summary>
        /// <returns>Número total de capturas.</returns>
        Task<int> CountAllCapturasAsync();

        /// <summary>
        /// Conta o número total de capturas de Pokémons por Mestre.
        /// </summary>
        /// <param name="mestrePokemonId">ID do Mestre Pokémon.</param>
        /// <returns>Número total de capturas.</returns>
        Task<int> CountCapturasByMestreAsync(int mestrePokemonId);

        /// <summary>
        /// Verifica se o Pokémon já foi capturado.
        /// </summary>
        /// <param name="mestrePokemonId"></param>
        /// <param name="pokemonId"></param>
        /// <returns></returns>
        Task<bool> IsPokemonAlreadyCapturedAsync(int mestrePokemonId, int pokemonId);
    }
}

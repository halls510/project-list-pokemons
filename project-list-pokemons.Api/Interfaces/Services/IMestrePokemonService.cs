using project_list_pokemons.Api.Dtos;

namespace project_list_pokemons.Api.Interfaces.Services
{
    public interface IMestrePokemonService
    {
        /// <summary>
        /// Cadastrar um Mestre Pokémon
        /// </summary>
        /// <param name="mestre"></param>
        /// <returns></returns>
        Task<MestrePokemonResponse> CadastrarMestreAsync(MestrePokemonRequest mestre);

        /// <summary>
        /// Busca um Mestre Pokémon por Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<MestrePokemonResponse?> BuscarMestrePorIdAsync(int id);

        /// <summary>
        /// Existe um Mestre pelo ID.
        /// </summary>
        /// <param name="mestrePokemonId">ID do Mestre.</param>
        /// <returns>Retorna True ou False se não encontrado.</returns>
        Task<bool> ExisteMestrePokemonAsync(int mestrePokemonId);
    }
}

using project_list_pokemons.Api.Dtos;
using project_list_pokemons.Api.Models;

namespace project_list_pokemons.Api.Parsers
{
    public static class MestrePokemonParser
    {
        /// <summary>
        /// Converte um DTO de entrada para a entidade MestrePokemon.
        /// </summary>
        /// <param name="request">DTO de entrada contendo os dados do Mestre Pokemon.</param>
        /// <returns>Uma instância de MestrePokemon ou lança uma exceção se a entrada for nula.</returns>
        /// <exception cref="ArgumentNullException">Lançada se o request for nulo.</exception>
        public static MestrePokemon ToEntity(MestrePokemonRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "O DTO de entrada não pode ser nulo.");
            }

            return new MestrePokemon
            {
                Nome = request.Nome ?? throw new ArgumentNullException(nameof(request.Nome), "O nome não pode ser nulo."),
                Idade = request.Idade,
                Cpf = request.Cpf ?? throw new ArgumentNullException(nameof(request.Cpf), "O CPF não pode ser nulo.")
            };
        }

        /// <summary>
        /// Converte uma entidade MestrePokemon para um DTO de saída.
        /// </summary>
        /// <param name="entity">Entidade MestrePokemon.</param>
        /// <returns>Uma instância de MestrePokemonResponse ou lança uma exceção se a entrada for nula.</returns>
        /// <exception cref="ArgumentNullException">Lançada se a entity for nula.</exception>
        public static MestrePokemonResponse ToDto(MestrePokemon entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "A entidade não pode ser nula.");
            }

            return new MestrePokemonResponse
            {
                Id = entity.Id,
                Nome = entity.Nome ?? throw new ArgumentNullException(nameof(entity.Nome), "O nome da entidade não pode ser nulo."),
                Idade = entity.Idade,
                Cpf = entity.Cpf ?? throw new ArgumentNullException(nameof(entity.Cpf), "O CPF da entidade não pode ser nulo.")
            };
        }
    }


}

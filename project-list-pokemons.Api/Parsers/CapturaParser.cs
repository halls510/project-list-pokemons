using project_list_pokemons.Api.Dtos;
using project_list_pokemons.Api.Models;

namespace project_list_pokemons.Api.Parsers
{
    public static class CapturaParser
    {
        /// <summary>
        /// Converte uma entidade Captura para um DTO de saída.
        /// </summary>
        public static CapturaResponse ToDto(Captura captura)
        {
            if (captura == null) throw new ArgumentNullException(nameof(captura));

            return new CapturaResponse
            {
                Id = captura.Id,
                MestrePokemonId = captura.MestrePokemonId,
                PokemonId = captura.PokemonId,
                PokemonNome = captura.Pokemon?.Name ?? string.Empty,
                DataCaptura = captura.DataCaptura
            };
        }

        /// <summary>
        /// Converte uma lista de entidades Captura para uma lista de DTOs de saída.
        /// </summary>
        public static List<CapturaResponse> ToDtoList(IEnumerable<Captura> capturas)
        {
            if (capturas == null) throw new ArgumentNullException(nameof(capturas));

            return capturas.Select(ToDto).ToList();
        }

        /// <summary>
        /// Converte um DTO de entrada para uma entidade Captura.
        /// </summary>
        public static Captura ToEntity(CapturaRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            return new Captura
            {
                MestrePokemonId = request.MestrePokemonId,
                PokemonId = request.PokemonId,
                DataCaptura = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Converte uma lista de DTOs de entrada para uma lista de entidades Captura.
        /// </summary>
        public static List<Captura> ToEntityList(IEnumerable<CapturaRequest> requests)
        {
            if (requests == null) throw new ArgumentNullException(nameof(requests));

            return requests.Select(ToEntity).ToList();
        }
    }
}

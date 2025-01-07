using project_list_pokemons.Api.Dtos;
using project_list_pokemons.Api.Models;

namespace project_list_pokemons.Api.Parsers
{
    public static class PokemonParser
    {
        /// <summary>
        /// Converte uma entidade Pokemon para um DTO de saída.
        /// </summary>
        public static PokemonResponse ToDto(Pokemon pokemon)
        {
            if (pokemon == null) throw new ArgumentNullException(nameof(pokemon));

            return new PokemonResponse
            {
                Id = pokemon.Id,
                Name = pokemon.Name,
                Height = pokemon.Height,
                Weight = pokemon.Weight,
                BaseExperience = pokemon.BaseExperience,
                SpriteBase64 = pokemon.SpriteBase64,
                Evolutions = pokemon.Evolutions.Select(e => new EvolutionResponse
                {
                    Id = e.Id,
                    Name = e.Name,
                    PokemonId = e.PokemonId
                }).ToList()
            };
        }

        /// <summary>
        /// Converte uma lista de entidades Pokemon para uma lista de DTOs de saída.
        /// </summary>
        public static List<PokemonResponse> ToDtoList(IEnumerable<Pokemon> pokemons)
        {
            if (pokemons == null) throw new ArgumentNullException(nameof(pokemons));

            return pokemons.Select(ToDto).ToList();
        }

        /// <summary>
        /// Converte um DTO de entrada para uma entidade Pokemon.
        /// </summary>
        public static Pokemon ToEntity(PokemonRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            return new Pokemon
            {
                Id = request.Id,
                Name = request.Name,
                Height = request.Height,
                Weight = request.Weight,
                BaseExperience = request.BaseExperience,
                SpriteBase64 = request.SpriteBase64,
                Hash = request.Hash,
                Evolutions = request.Evolutions?.Select(e => new Evolution
                {
                    Id = e.Id,
                    Name = e.Name,
                    PokemonId = e.PokemonId
                }).ToList() ?? new List<Evolution>()
            };
        }

        /// <summary>
        /// Converte uma lista de DTOs de entrada para uma lista de entidades Pokemon.
        /// </summary>
        public static List<Pokemon> ToEntityList(IEnumerable<PokemonRequest> requests)
        {
            if (requests == null) throw new ArgumentNullException(nameof(requests));

            return requests.Select(ToEntity).ToList();
        }
    }
}

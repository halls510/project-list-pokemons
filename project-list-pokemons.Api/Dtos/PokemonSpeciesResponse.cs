using System.Text.Json.Serialization;

namespace project_list_pokemons.Api.Dtos
{
    public class PokemonSpeciesResponse
    {
        [JsonPropertyName("evolution_chain")]
        public EvolutionChain? EvolutionChain { get; set; }
    }
}

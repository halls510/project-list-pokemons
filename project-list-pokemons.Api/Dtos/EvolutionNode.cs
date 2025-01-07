using System.Text.Json.Serialization;

namespace project_list_pokemons.Api.Dtos
{
    public class EvolutionNode
    {
        [JsonPropertyName("species")]
        public PokemonSpecies Species { get; set; }

        [JsonPropertyName("evolves_to")]
        public List<EvolutionNode> EvolvesTo { get; set; } = new();
    }
}

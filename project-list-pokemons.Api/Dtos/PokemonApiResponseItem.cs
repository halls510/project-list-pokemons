using project_list_pokemons.Api.Services;
using System.Text.Json.Serialization;

namespace project_list_pokemons.Api.Dtos
{
    public class PokemonApiResponseItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("height")]
        public int Height { get; set; }
        [JsonPropertyName("weight")]
        public int Weight { get; set; }
        [JsonPropertyName("base_experience")]
        public int BaseExperience { get; set; }
        [JsonPropertyName("species")]
        public PokemonSpecies Species { get; set; } = new PokemonSpecies();
        [JsonPropertyName("sprites")]
        public Sprites Sprites { get; set; } = new Sprites();
    }
}

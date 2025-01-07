using System.Text.Json.Serialization;

namespace project_list_pokemons.Api.Dtos
{
    public class PokemonSpecies
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }
}

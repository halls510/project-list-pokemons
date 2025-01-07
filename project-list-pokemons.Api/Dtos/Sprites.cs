using System.Text.Json.Serialization;

namespace project_list_pokemons.Api.Dtos
{
    public class Sprites
    {
        [JsonPropertyName("front_default")]
        public string FrontDefault { get; set; } = string.Empty;
    }
}

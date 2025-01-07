using System.Text.Json.Serialization;

namespace project_list_pokemons.Api.Dtos
{
    public class EvolutionChain
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }
}

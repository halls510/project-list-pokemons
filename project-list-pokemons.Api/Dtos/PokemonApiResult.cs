using System.Text.Json.Serialization;

namespace project_list_pokemons.Api.Dtos
{
    public class PokemonApiResult
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}

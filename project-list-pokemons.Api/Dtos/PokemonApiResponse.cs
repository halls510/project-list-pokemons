using System.Text.Json.Serialization;

namespace project_list_pokemons.Api.Dtos
{
    public class PokemonApiResponse
    {
        [JsonPropertyName("count")]
        public int? Count { get; set; } = null;
        [JsonPropertyName("results")]
        public List<PokemonApiResult> Results { get; set; } = new();
    }
}

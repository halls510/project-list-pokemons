using System.Text.Json.Serialization;

namespace project_list_pokemons.Api.Dtos
{
    public class PokemonApiResponseTotal
    {
        [JsonPropertyName("count")]
        public int? Count { get; set; } = null;
    }
}

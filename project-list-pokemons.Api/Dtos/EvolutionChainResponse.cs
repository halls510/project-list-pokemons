using System.Text.Json.Serialization;

namespace project_list_pokemons.Api.Dtos
{
    public class EvolutionChainResponse
    {
        [JsonPropertyName("chain")]
        public EvolutionNode? Chain { get; set; }
    }
}

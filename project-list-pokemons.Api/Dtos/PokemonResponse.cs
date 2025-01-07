namespace project_list_pokemons.Api.Dtos
{
    public class PokemonResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Height { get; set; }
        public int Weight { get; set; }
        public int BaseExperience { get; set; }
        public string SpriteBase64 { get; set; } = string.Empty;
        public List<EvolutionResponse> Evolutions { get; set; } = new();
    }

}

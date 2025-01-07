namespace project_list_pokemons.Api.Dtos
{
    public class EvolutionResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int PokemonId { get; set; }
    }
}

namespace project_list_pokemons.Api.Dtos
{
    public class EvolutionRequest
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int PokemonId { get; set; }
    }
}

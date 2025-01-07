namespace project_list_pokemons.Api.Models
{
    public class Evolution
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int PokemonId { get; set; }
        public Pokemon Pokemon { get; set; }
    }

}

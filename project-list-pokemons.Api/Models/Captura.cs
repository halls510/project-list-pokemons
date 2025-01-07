namespace project_list_pokemons.Api.Models
{
    public class Captura
    {
        public int Id { get; set; }
        public int MestrePokemonId { get; set; }
        public MestrePokemon MestrePokemon { get; set; } = null!;
        public int PokemonId { get; set; }
        public Pokemon Pokemon { get; set; } = null!;

        public DateTime DataCaptura { get; set; } = DateTime.UtcNow;
    }

}

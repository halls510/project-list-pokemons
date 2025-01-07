namespace project_list_pokemons.Api.Dtos
{
    public class CapturaResponse
    {
        public int Id { get; set; }
        public int MestrePokemonId { get; set; }
        public int PokemonId { get; set; }
        public string PokemonNome { get; set; } = string.Empty;
        public DateTime DataCaptura { get; set; }
    }

}

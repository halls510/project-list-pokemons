namespace project_list_pokemons.Api.Models
{
    public class MestrePokemon
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int Idade { get; set; }
        public string Cpf { get; set; } = string.Empty;

        public ICollection<Captura> Capturas { get; set; } = new List<Captura>();
    }

}

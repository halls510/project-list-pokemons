namespace project_list_pokemons.Api.Dtos
{
    public class MestrePokemonResponse
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int Idade { get; set; }
        public string Cpf { get; set; } = string.Empty;
    }

}

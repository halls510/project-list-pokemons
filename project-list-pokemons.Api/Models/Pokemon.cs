namespace project_list_pokemons.Api.Models
{
    public class Pokemon
    {
        public int Id { get; set; } // ID do Pokémon na PokéAPI
        public string Name { get; set; } = string.Empty;
        public int Height { get; set; } // Altura do Pokémon em decímetros (dm)        
        public int Weight { get; set; } // Peso do Pokémon em hectogramas (hg)
        public int BaseExperience { get; set; } // Experiência base concedida pelo Pokémon
        public string SpriteBase64 { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;

        public ICollection<Captura> Capturas { get; set; } = new List<Captura>();
        public ICollection<Evolution> Evolutions { get; set; } = new List<Evolution>();

    }
}

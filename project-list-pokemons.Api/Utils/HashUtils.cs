using project_list_pokemons.Api.Models;
using System.Security.Cryptography;
using System.Text;

namespace project_list_pokemons.Api.Utils
{
    public static class HashUtils
    {
        public static string GenerateHash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }

        public static string GeneratePokemonHash(Pokemon pokemon)
        {
            var dataToHash = $"{pokemon.Name}|{pokemon.SpriteBase64}|{string.Join(",", pokemon.Evolutions.Select(e => e.Name))}";
            return GenerateHash(dataToHash);
        }
    }
}

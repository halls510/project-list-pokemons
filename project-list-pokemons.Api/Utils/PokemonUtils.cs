using project_list_pokemons.Api.Models;
using project_list_pokemons.Api.Services;

namespace project_list_pokemons.Api.Utils
{
    public static class PokemonUtils
    {
        private static readonly string DefaultImagePath = "/app/Resources/Images/default-image.png";
        private static readonly string DefaultImageBase64 = File.Exists(DefaultImagePath)
            ? Convert.ToBase64String(File.ReadAllBytes(DefaultImagePath))
            : throw new FileNotFoundException($"O arquivo default-image.png não foi encontrado no caminho: {DefaultImagePath}");

        /// <summary>
        /// Converte uma imagem de uma URL fornecida para uma string Base64. 
        /// Caso a URL seja inválida, a imagem não seja acessível, o tipo de mídia não seja permitido, 
        /// ou ocorra algum erro durante o processamento, retorna uma imagem padrão em Base64.
        /// </summary>
        /// <param name="httpClient">Instância do HttpClient usada para buscar a imagem.</param>
        /// <param name="imageUrl">URL da imagem que será convertida para Base64.</param>
        /// <param name="logger">Instância do ILogger para registrar mensagens de log.</param>
        /// <returns>Uma string Base64 representando a imagem ou a imagem padrão caso a conversão falhe.</returns>
        public static async Task<string> ConvertImageToBase64(IHttpClientWrapper httpClient, string imageUrl, int pokemonId, ILogger logger)
        {
            // Tipos de imagem permitidos
            var allowedMediaTypes = new[] { "image/png", "image/jpeg", "image/jpg", "image/gif" };

            try
            {
                // Verificar se a URL é nula ou vazia
                if (string.IsNullOrWhiteSpace(imageUrl))
                {
                    logger.LogWarning("URL inválida de Imagem para o PokemonId {pokemonId} - URL: {ImageUrl}", pokemonId, imageUrl ?? "NULL");
                    return DefaultImageBase64;
                }

                // Verificar se a URL é válida
                if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uriResult))
                {
                    logger.LogWarning("URL inválida de Imagem para o PokemonId {pokemonId} - URL: {ImageUrl}", pokemonId, imageUrl);
                    return DefaultImageBase64;
                }

                // Fazer a requisição HTTP
                using var response = await httpClient.GetAsync(uriResult);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("Falha ao obter imagem. Código HTTP: {StatusCode}, URL: {ImageUrl}, PokemonId: {pokemonId}", response.StatusCode, imageUrl, pokemonId);
                    return DefaultImageBase64;
                }

                // Verificar o Content-Type da resposta
                var contentType = response.Content.Headers.ContentType?.MediaType;
                if (string.IsNullOrEmpty(contentType) || !allowedMediaTypes.Contains(contentType))
                {
                    logger.LogWarning("Tipo de mídia não permitido: {MediaType}. URL: {ImageUrl}. PokemonId: {pokemonId}", contentType, imageUrl, pokemonId);
                    return DefaultImageBase64;
                }

                // Obter os bytes da imagem
                var imageBytes = await response.Content.ReadAsByteArrayAsync();

                // Verificar se a imagem não está vazia
                if (imageBytes.Length == 0)
                {
                    logger.LogWarning("A imagem está vazia. URL: {ImageUrl}. PokemonId: {pokemonId}", imageUrl, pokemonId);
                    return DefaultImageBase64;
                }

                // Converter para Base64
                return Convert.ToBase64String(imageBytes);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Erro ao converter imagem para Base64. URL: {ImageUrl}. PokemonId: {pokemonId}", imageUrl, pokemonId);
                return DefaultImageBase64;
            }
        }

        /// <summary>
        /// Extrai o ID de um Pokémon a partir da URL fornecida.
        /// </summary>
        public static int ExtractIdFromUrl(string url)
        {
            var segments = url.TrimEnd('/').Split('/');
            return int.Parse(segments[^1]);
        }
    }
}

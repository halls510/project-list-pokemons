namespace project_list_pokemons.Api.Dtos
{
    public class ApiResponseToken
    {  
        /// <summary>
       /// Token.
       /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Código do status para facilitar a identificação.
        /// </summary>
        public string StatusCode { get; set; }

        /// <summary>
        /// Detalhes adicionais (opcional).
        /// </summary>
        public string? Details { get; set; }

        /// <summary>
        /// Construtor padrão.
        /// </summary>
        public ApiResponseToken(string token, string statusCode, string? details = null)
        {
            Token = token;
            StatusCode = statusCode;
            Details = details;
        }
    }
}

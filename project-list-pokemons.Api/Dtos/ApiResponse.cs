namespace project_list_pokemons.Api.Dtos
{
    public class ApiResponse
    {  
        /// <summary>
        /// Mensagem descritiva do erro.
        /// </summary>
        public string Message { get; set; }

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
        public ApiResponse(string message, string statusCode, string? details = null)
        {
            Message = message;
            StatusCode = statusCode;
            Details = details;
        }
    }
}

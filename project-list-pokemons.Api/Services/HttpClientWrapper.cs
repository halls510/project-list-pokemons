namespace project_list_pokemons.Api.Services
{
    public class HttpClientWrapper : IHttpClientWrapper
    {
        private readonly HttpClient _httpClient;

        public HttpClientWrapper(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetStringAsync(string url)
        {
            return await _httpClient.GetStringAsync(url);
        }

        public async Task<string> GetStringAsync(string? requestUri, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(requestUri))
            {
                throw new ArgumentNullException(nameof(requestUri), "A URI fornecida não pode ser nula ou vazia.");
            }

            return await _httpClient.GetStringAsync(requestUri, cancellationToken);
        }

        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            return await _httpClient.GetAsync(url);
        }

        public async Task<HttpResponseMessage> GetAsync(Uri? requestUri)
        {
            if (requestUri == null)
            {
                throw new ArgumentNullException(nameof(requestUri), "A URI fornecida não pode ser nula.");
            }

            return await _httpClient.GetAsync(requestUri);
        }
    }

}

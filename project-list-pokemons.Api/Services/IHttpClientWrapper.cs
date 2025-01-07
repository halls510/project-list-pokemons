namespace project_list_pokemons.Api.Services
{
    public interface IHttpClientWrapper
    {
        Task<string> GetStringAsync(string url);
        Task<string> GetStringAsync(string? requestUri, CancellationToken cancellationToken);
        Task<HttpResponseMessage> GetAsync(string url);
        Task<HttpResponseMessage> GetAsync(Uri? requestUri);
    }

}

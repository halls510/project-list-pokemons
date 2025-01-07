namespace project_list_pokemons.Api.Interfaces.Utils
{
    public interface IRedisCacheHelper
    {
        /// <summary>
        /// Obtém um valor por Key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<string?> GetValueAsync(string key);

        /// <summary>
        /// Adiciona um valor ao Redis Cache
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        Task SetValueAsync(string key, string? value, TimeSpan expiration);
    }
}

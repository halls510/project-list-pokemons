using project_list_pokemons.Api.Interfaces.Utils;
using StackExchange.Redis;

namespace project_list_pokemons.Api.Utils
{
    public class RedisCacheHelper : IRedisCacheHelper
    {
        private readonly IDatabase _cache;
        private readonly ILogger<RedisCacheHelper> _logger;

        public RedisCacheHelper(IConnectionMultiplexer connectionMultiplexer, ILogger<RedisCacheHelper> logger)
        {
            _cache = connectionMultiplexer.GetDatabase();
            _logger = logger;
        }

        /// <summary>
        /// Obtém um valor por Key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<string?> GetValueAsync(string key)
        {
            try
            {
                var value = await _cache.StringGetAsync(key);
                if (!value.IsNullOrEmpty)
                {
                    _logger.LogInformation("Cache HIT para chave: {Key}", key);
                    return value;
                }

                _logger.LogInformation("Cache MISS para chave: {Key}", key);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter valor do Redis para chave: {Key}", key);
                return null;
            }
        }

        /// <summary>
        /// Adiciona um valor ao Redis Cache
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        public async Task SetValueAsync(string key, string? value, TimeSpan expiration)
        {
            try
            {
                if (value == null)
                {
                    // Remove do cache
                    await _cache.KeyDeleteAsync(key);
                    _logger.LogInformation("Cache invalidado para chave: {Key}", key);
                }
                else
                {
                    // Armazena no cache
                    await _cache.StringSetAsync(key, value, expiration);
                    _logger.LogInformation("Valor armazenado no cache com chave: {Key}", key);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao armazenar valor no Redis para chave: {Key}", key);
            }
        }
    }

}

using System.Text.Json;
using CSharpExtensions;
using MDP.Caching.Contract;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace MDP.Caching
{
    public class CacheClient : ICacheClient
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<CacheClient> _logger;

        public CacheClient(IDistributedCache distributedCache, ILogger<CacheClient> logger)
        {
            _distributedCache = distributedCache;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var cachedItem = await _distributedCache.GetStringAsync(key);
            if (cachedItem.IsNotNullOrEmpty())
            {
                try
                {
                    return JsonSerializer.Deserialize<T>(cachedItem);
                }
                catch (Exception e)
                {
                    _logger.LogDebug(e.Message, e);
                }
            }

            return default;
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? cacheExpiration)
        {
            if (value != null)
            {
                try
                {
                    var valueToCache = JsonSerializer.Serialize(value);

                    return _distributedCache.SetStringAsync(key, valueToCache, new DistributedCacheEntryOptions()
                    {
                        AbsoluteExpirationRelativeToNow = cacheExpiration
                    });
                }
                catch (Exception e)
                {
                    _logger.LogDebug(e.Message, e);
                }
            }

            return Task.CompletedTask;
        }
    }
}
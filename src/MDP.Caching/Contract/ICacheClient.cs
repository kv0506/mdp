namespace MDP.Caching.Contract;

public interface ICacheClient
{
    Task<T?> GetAsync<T>(string key);

    Task SetAsync<T>(string key, T value, TimeSpan? cacheExpiration);
}
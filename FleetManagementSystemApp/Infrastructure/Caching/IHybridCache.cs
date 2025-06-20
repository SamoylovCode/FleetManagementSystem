namespace FleetManagementSystemApp.Infrastructure.Caching;

public interface IHybridCache
{
    public Task<T> GetOrAddAsync<T>(Func<Task<T>> factory, string key, TimeSpan? ttl, string? prefix);
    Task RemoveAsync(string key, string? prefix);
    Task RemoveByPrefixAsync(string prefix);
}

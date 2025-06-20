using LazyCache;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Text;
using ILogger = Serilog.ILogger;

namespace FleetManagementSystemApp.Infrastructure.Caching;

public class HybridCache : IHybridCache
{
    private const string InvalidateChannel = "cache_invalidate";

    private readonly IAppCache _memoryCache;
    private readonly IDistributedCache _redis;
    private readonly TimeSpan _defaultTtl;
    private readonly ILogger _logger;
    private readonly IConnectionMultiplexer _redisConnection;

    public HybridCache(IAppCache memoryCache, IDistributedCache redis, IConfiguration config, ILogger logger, IConnectionMultiplexer redisConnection)
    {
        _memoryCache = memoryCache;
        _redis = redis;
        _defaultTtl = TimeSpan.FromSeconds(config.GetValue<int>("CacheOptions:DefaultTtlSeconds"));
        _logger = logger;
        _redisConnection = redisConnection;

        var subscriber = _redisConnection.GetSubscriber();
        subscriber.Subscribe(InvalidateChannel, (channel, message) => 
        {
            var msg = (string)message;
            if (msg.StartsWith("prefix:"))
            {
                var prefix = msg.Substring("prefix:".Length);
                _ = Task.Run(() => InvalidateLocalByPrefix(prefix));
                _logger.Information("Local cache invalidated via Pub/Sub: {Message}", msg);
            }
            else
            {
                InvalidateLocalKey(msg);
                _logger.Information("Local cache invalidated via Pub/Sub: {Message}", msg);
            }
        });
        _logger.Information("HybridCache initialized and subscribed to {Channel}", InvalidateChannel);
    }

    private IDatabase RedisDb
    {
        get
        {
            return _redisConnection.GetDatabase();
        }
    }

    private string GetSpecifiedPrefix(string prefix)
    {
        return $"__prefix:{prefix}__keys";
    }

    public async Task<T> GetOrAddAsync<T>(Func<Task<T>> factory, string key, TimeSpan? ttl = null, string? prefix = null)
    {
        if(_memoryCache.TryGetValue(key, out T cached))
        {
            _logger.Debug("MemoryCache hit: {Key}", key);
            return cached;
        }

        var data = await _redis.GetAsync(key);

        if (data is not null)
        {
            var serializedData = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data));
            _memoryCache.Add(key, serializedData, ttl ?? _defaultTtl);
            _logger.Debug("RedisCache hit: {Key}", key);
            return serializedData!;
        }
        else
        {
            _logger.Debug("RedisCache miss: {Key}", key);
        }

        var value = await factory();

        var serializedValue = JsonConvert.SerializeObject(value);
        var bytesValue = Encoding.UTF8.GetBytes(serializedValue);
        await _redis.SetAsync(key, bytesValue, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl ?? _defaultTtl
        });

        if (!string.IsNullOrWhiteSpace(prefix))
        {
            await RedisDb.SetAddAsync(GetSpecifiedPrefix(prefix), key);
            _logger.Information($"Added {prefix} in Redis store", prefix);
        }

        _memoryCache.Add(key, value, ttl ?? _defaultTtl);
        _logger.Information("Cached new value under key: {Key}", key);

        return value;
    }

    public async Task RemoveAsync(string key, string? prefix = null)
    {
        InvalidateLocalKey(key);

        await _redis.RemoveAsync(key);
        _logger.Information("Removed cache entry: {Key}", key);

        if (!string.IsNullOrEmpty(prefix))
        {
            await RedisDb.SetRemoveAsync(GetSpecifiedPrefix(prefix), key);
            _logger.Information("Removed cache entry by prefix: {Prefix}", prefix);
        }

        var sub = _redisConnection.GetSubscriber();
        await sub.PublishAsync(InvalidateChannel, key);
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        var specifiedKey = GetSpecifiedPrefix(prefix);
        var members = await RedisDb.SetMembersAsync(specifiedKey);

        foreach (var member in members)
        {
            var key = member.ToString()!;
            InvalidateLocalKey(key);
            await _redis.RemoveAsync(key);
            _logger.Information("Removed cache entry by prefix: {Key}", key);
        }

        await RedisDb.KeyDeleteAsync(specifiedKey);

        var sub = _redisConnection.GetSubscriber();
        await sub.PublishAsync(InvalidateChannel, $"prefix:{prefix}");
    }

    public void InvalidateLocalKey(string key)
    {
        _memoryCache.Remove(key);
    }

    public async Task InvalidateLocalByPrefix(string prefix)
    {
        var specifiedKey = GetSpecifiedPrefix(prefix);
        var members = await RedisDb.SetMembersAsync(specifiedKey);

        foreach(var member in members)
        {
            var key = member.ToString()!;
            _memoryCache.Remove(key);
        }
    }
}

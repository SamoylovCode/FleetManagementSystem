using FleetManagementSystemApp.Common.Extensions;
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

    public HybridCache(
        IAppCache memoryCache,
        IDistributedCache redis,
        IConfiguration config,
        ILogger logger,
        IConnectionMultiplexer redisConnection)
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

    public async Task<T> GetOrAddAsync<T>(
        Func<Task<T>> factory,
        string cacheKey,
        TimeSpan? ttl = null,
        string? prefix = null)
    {
        if(_memoryCache.TryGetValue(cacheKey, out T cached))
        {
            _logger.Debug("MemoryCache hit: {Key}", cacheKey);
            return cached;
        }

        var data = await _redis.GetAsync(cacheKey);

        if (data is not null)
        {
            var serializedData = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data));
            _memoryCache.Add(cacheKey, serializedData, ttl ?? _defaultTtl);
            _logger.Debug("RedisCache hit: {CacheKey}", cacheKey);
            return serializedData!;
        }
        else
        {
            _logger.Debug("RedisCache miss: {CacheKey}", cacheKey);
        }

        var value = await factory();

        if (value == null)
        {
            _logger.Debug("Factory returned null for key {CacheKey}, skipping cache.", cacheKey);
            return default!;
        }

        var serializedValue = JsonConvert.SerializeObject(value);
        var bytesValue = Encoding.UTF8.GetBytes(serializedValue);
        await _redis.SetAsync(cacheKey, bytesValue, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl ?? _defaultTtl
        });

        if (!string.IsNullOrWhiteSpace(prefix))
        {
            await RedisDb.SetAddAsync(GetSpecifiedPrefix(prefix), cacheKey);
            var added = await RedisDb.SetContainsAsync(GetSpecifiedPrefix(prefix), cacheKey);
            if (!added)
            {
                _logger.Error("Failed to add key {CacheKey} to prefix container {Prefix}", cacheKey, prefix);
                throw new InvalidOperationException($"Failed to add key {cacheKey} to prefix {prefix}");
            }
            _logger.Information("Added key {CacheKey} to {Prefix} in Redis store", cacheKey, prefix);
        }

        _memoryCache.Add(cacheKey, value, ttl ?? _defaultTtl);
        _logger.Information("Cached new value under key: {CacheKey}", cacheKey);

        return value;
    }

    public async Task RemoveAsync(string cacheKey, string? prefix = null)
    {
        InvalidateLocalKey(cacheKey);

        await _redis.RemoveAsync(cacheKey);
        _logger.Information("Removed cache entry: {CacheKey}", cacheKey);

        if (!string.IsNullOrEmpty(prefix))
        {
            await RedisDb.SetRemoveAsync(GetSpecifiedPrefix(prefix), cacheKey);
            _logger.Information("Removed cache entry by prefix: {Prefix}", prefix);
        }

        var sub = _redisConnection.GetSubscriber();
        await sub.PublishAsync(InvalidateChannel, cacheKey);
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        var specifiedKey = GetSpecifiedPrefix(prefix);
        var members = await RedisDb.SetMembersAsync(specifiedKey);

        foreach (var member in members)
        {
            var cacheKey = member.ToString()!;
            InvalidateLocalKey(cacheKey);
            await _redis.RemoveAsync(cacheKey);
            _logger.Information("Removed cache entry by prefix: {CacheKey}", cacheKey);
        }

        await RedisDb.KeyDeleteAsync(specifiedKey);

        var sub = _redisConnection.GetSubscriber();
        await sub.PublishAsync(InvalidateChannel, $"prefix:{prefix}");
    }

    public void InvalidateLocalKey(string cacheKey)
    {
        _memoryCache.Remove(cacheKey);
    }

    public async Task InvalidateLocalByPrefix(string prefix)
    {
        var specifiedKey = GetSpecifiedPrefix(prefix);
        var members = await RedisDb.SetMembersAsync(specifiedKey);

        foreach(var member in members)
        {
            var cacheKey = member.ToString()!;
            _memoryCache.Remove(cacheKey);
        }
    }

    // For debuging
    public async Task DumpPrefixesAsync()
    {
        var server = _redisConnection.GetServer(_redisConnection.GetEndPoints().First());

        // Get all keys with prefixes
        foreach (var cacheKey in server.Keys(pattern: "__prefix:*__keys"))
        {
            Console.WriteLine($"Prefix key container: {cacheKey}");
            var members = await RedisDb.SetMembersAsync(cacheKey);
            foreach (var member in members)
            {
                Console.WriteLine($"  -> {member}");
            }
        }
    }
}
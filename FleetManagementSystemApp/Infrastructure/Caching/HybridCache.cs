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

    private IDatabase RedisDb => _redisConnection.GetDatabase();

    private string NormalizeCacheKey(string cacheKey)
    {
        if (string.IsNullOrWhiteSpace(cacheKey))
        {
            throw new ArgumentNullException(nameof(cacheKey));
        }

        return cacheKey.ToLowerInvariant();
    }

    private string NormalizePrefix(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            return string.Empty;
        }

        return prefix.ToLowerInvariant();
    }

    // Контейнер префикса в Redis: "__prefix:{normalizedPrefix}__keys"
    private string GetSpecifiedPrefixContainer(string prefix)
    {
        return $"__prefix:{NormalizePrefix(prefix)}__keys";
    }

    // Композитный ключ, используемый в memory и redis.
    // Если prefix задан — finalKey = "{normalizedPrefix}:{normalizedCacheKey}",
    // иначе finalKey = "{normalizedCacheKey}"
    private string GetCompositeKey(string cacheKey, string? prefix)
    {
        var key = NormalizeCacheKey(cacheKey);
        if (string.IsNullOrWhiteSpace(prefix))
        {
            return key;
        }

        return $"{NormalizePrefix(prefix)}:{key}";
    }

    public async Task<T> GetOrAddAsync<T>(
        Func<Task<T>> factory,
        string cacheKey,
        TimeSpan? ttl = null,
        string? prefix = null)
    {
        if (factory is null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        if (string.IsNullOrWhiteSpace(cacheKey))
        {
            throw new ArgumentNullException(nameof(cacheKey));
        }

        var finalKey = GetCompositeKey(cacheKey, prefix);

        // Memory cache (local)
        if (_memoryCache.TryGetValue(finalKey, out T cached))
        {
            //_logger.Debug("MemoryCache hit: {Key}", finalKey);
            return cached;
        }

        var data = await _redis.GetAsync(finalKey);
        if (data is not null)
        {
            var json = Encoding.UTF8.GetString(data);
            try
            {
                var deserialized = JsonConvert.DeserializeObject<T>(json);

                if (deserialized == null)
                {
                    _logger.Warning("Redis key {CacheKey} deserialized to null (treat as miss).", finalKey);
                }

                _memoryCache.Add(finalKey, deserialized, ttl ?? _defaultTtl);
                //_logger.Debug("RedisCache hit: {CacheKey}", finalKey);
                return deserialized!;
            }
            catch (JsonException jex)
            {
                _logger.Error(jex, "Failed to deserialize cache key {CacheKey} to {Type}. Raw JSON: {Json}", finalKey, typeof(T).FullName, json);
                await _redis.RemoveAsync(finalKey);
                _memoryCache.Remove(finalKey);
                _logger.Warning("Corrupt cache entry removed: {CacheKey}", finalKey);
            }
        }
        else
        {
            _logger.Debug("RedisCache miss: {CacheKey}", finalKey);
        }

        // Factory
        var value = await factory();
        if (value == null)
        {
            _logger.Debug("Factory returned null for key {CacheKey}, skipping cache.", finalKey);
            return default!;
        }

        // Cериализация только для Redis, в memory cache помещается неизменяемый (не сериализованный) объект
        var serializedValue = JsonConvert.SerializeObject(value);

        // Redis
        await _redis.SetAsync(
            finalKey,
            Encoding.UTF8.GetBytes(serializedValue),
            new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl ?? _defaultTtl
        });

        if (!string.IsNullOrWhiteSpace(prefix))
        {
            var prefixContainer = GetSpecifiedPrefixContainer(prefix);
            await RedisDb.SetAddAsync(prefixContainer, finalKey);
            var added = await RedisDb.SetContainsAsync(prefixContainer, finalKey);

            if (!added)
            {
                _logger.Error("Failed to add key {CacheKey} to prefix container {Prefix}", finalKey, NormalizePrefix(prefix));
                throw new InvalidOperationException($"Failed to add key {finalKey} to prefix {NormalizePrefix(prefix)}");
            }

            _logger.Information("Added key {CacheKey} to {Prefix} in Redis store", finalKey, NormalizePrefix(prefix));
        }

        // В memory cache кладётся value, а не десериализованная копия
        var serialized = JsonConvert.SerializeObject(value);
        var clone = JsonConvert.DeserializeObject<T>(serialized)!;

        _memoryCache.Add(finalKey, clone, ttl ?? _defaultTtl);
        _logger.Information("Cached new value under key: {CacheKey}", finalKey);

        return clone;
    }

    public async Task RemoveAsync(string cacheKey, string? prefix = null)
    {
        if (string.IsNullOrWhiteSpace(cacheKey))
        {
            return;
        }

        var finalKey = GetCompositeKey(cacheKey, prefix);
        InvalidateLocalKey(finalKey);

        await _redis.RemoveAsync(finalKey);
        _logger.Information("Removed cache entry: {CacheKey}", finalKey);

        if (!string.IsNullOrEmpty(prefix))
        {
            var container = GetSpecifiedPrefixContainer(prefix);
            await RedisDb.SetRemoveAsync(container, finalKey);
            _logger.Information("Removed cache entry from prefix container: {Prefix}", prefix);
        }

        var sub = _redisConnection.GetSubscriber();
        await sub.PublishAsync(InvalidateChannel, finalKey);
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix)) return;

        var normalizedPrefix = NormalizePrefix(prefix);
        var specifiedKey = GetSpecifiedPrefixContainer(normalizedPrefix);
        var members = await RedisDb.SetMembersAsync(specifiedKey);
        //_logger.Debug("RemoveByPrefix: prefix={Prefix}, membersCount={Count}", normalizedPrefix, members.Length);

        foreach (var member in members)
        {
            var cacheKey = member.ToString()!;
            InvalidateLocalKey(cacheKey);
            await _redis.RemoveAsync(cacheKey);
            _logger.Information("Removed cache entry by prefix: {CacheKey}", cacheKey);
        }

        await RedisDb.KeyDeleteAsync(specifiedKey);

        var sub = _redisConnection.GetSubscriber();
        await sub.PublishAsync(InvalidateChannel, $"prefix:{normalizedPrefix}");
    }

    public void InvalidateLocalKey(string cacheKey)
    {
        if (string.IsNullOrWhiteSpace(cacheKey))
        {
            return;
        }

        _memoryCache.Remove(cacheKey.ToLowerInvariant());
    }

    public async Task InvalidateLocalByPrefix(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            return;
        }

        var specifiedKey = GetSpecifiedPrefixContainer(prefix);
        var members = await RedisDb.SetMembersAsync(specifiedKey);

        foreach (var member in members)
        {
            var cacheKey = member.ToString()!;
            _memoryCache.Remove(cacheKey);
        }
    }

    // For debugging
    public async Task DumpPrefixesAsync()
    {
        var server = _redisConnection.GetServer(_redisConnection.GetEndPoints().First());

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
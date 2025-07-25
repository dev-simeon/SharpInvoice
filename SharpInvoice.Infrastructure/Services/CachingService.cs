using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SharpInvoice.Core.Interfaces.Services;
using System.Text.Json;

namespace SharpInvoice.Infrastructure.Services;

/// <summary>
/// Service for caching operations using the distributed cache.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CachingService"/> class.
/// </remarks>
/// <param name="cache">The distributed cache.</param>
/// <param name="logger">The logger.</param>
public class CachingService(IDistributedCache cache, ILogger<CachingService> logger) : ICachingService
{
    private readonly IDistributedCache _cache = cache;
    private readonly ILogger<CachingService> _logger = logger;

    /// <summary>
    /// Gets a cached item by key.
    /// </summary>
    /// <typeparam name="T">The type of the item to retrieve.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cached item, or null if not found.</returns>
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var cachedValue = await _cache.GetStringAsync(key, cancellationToken);
            if (string.IsNullOrEmpty(cachedValue))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(cachedValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cached item with key: {Key}", key);
            return default;
        }
    }

    /// <summary>
    /// Sets a cached item.
    /// </summary>
    /// <typeparam name="T">The type of the item to cache.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="absoluteExpirationRelativeToNow">The time after which the cache entry should expire.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNow = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow ?? TimeSpan.FromMinutes(30)
            };

            await _cache.SetStringAsync(key, serializedValue, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching item with key: {Key}", key);
        }
    }

    /// <summary>
    /// Removes a cached item.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cached item with key: {Key}", key);
        }
    }

    /// <summary>
    /// Gets a cached item by key, or creates it if it doesn't exist.
    /// </summary>
    /// <typeparam name="T">The type of the item to retrieve or create.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">Factory method to create the item if not cached.</param>
    /// <param name="absoluteExpirationRelativeToNow">The time after which the cache entry should expire.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cached or newly created item.</returns>
    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? absoluteExpirationRelativeToNow = null,
        CancellationToken cancellationToken = default)
    {
        var cachedValue = await GetAsync<T>(key, cancellationToken);
        if (cachedValue != null)
        {
            return cachedValue;
        }

        var newValue = await factory();
        if (newValue != null)
        {
            await SetAsync(key, newValue, absoluteExpirationRelativeToNow, cancellationToken);
        }

        return newValue;
    }
} 
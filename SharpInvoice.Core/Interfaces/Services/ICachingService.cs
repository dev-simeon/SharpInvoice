namespace SharpInvoice.Core.Interfaces.Services;

/// <summary>
/// Interface for caching operations.
/// </summary>
public interface ICachingService
{
    /// <summary>
    /// Gets a cached item by key.
    /// </summary>
    /// <typeparam name="T">The type of the item to retrieve.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cached item, or null if not found.</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a cached item.
    /// </summary>
    /// <typeparam name="T">The type of the item to cache.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="absoluteExpirationRelativeToNow">The time after which the cache entry should expire.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNow = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a cached item.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a cached item by key, or creates it if it doesn't exist.
    /// </summary>
    /// <typeparam name="T">The type of the item to retrieve or create.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">Factory method to create the item if not cached.</param>
    /// <param name="absoluteExpirationRelativeToNow">The time after which the cache entry should expire.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cached or newly created item.</returns>
    Task<T> GetOrCreateAsync<T>(
        string key, 
        Func<Task<T>> factory,
        TimeSpan? absoluteExpirationRelativeToNow = null,
        CancellationToken cancellationToken = default);
} 
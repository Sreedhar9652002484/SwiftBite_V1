using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SwiftBite.PaymentService.Application.Common.Interfaces;
using System.Text.Json;

namespace SwiftBite.PaymentService.Infrastructure.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(
        IDistributedCache cache,
        ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(
        string key, CancellationToken ct = default)
    {
        try
        {
            var json = await _cache.GetStringAsync(key, ct);
            if (json is null) return default;
            _logger.LogInformation(
                "⚡ Cache HIT: {Key}", key);
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Cache GET failed: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(
        string key, T value,
        TimeSpan? expiry = null,
        CancellationToken ct = default)
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow =
                    expiry ?? TimeSpan.FromMinutes(5)
            };
            await _cache.SetStringAsync(key,
                JsonSerializer.Serialize(value),
                options, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Cache SET failed: {Key}", key);
        }
    }

    public async Task RemoveAsync(
        string key, CancellationToken ct = default)
    {
        try { await _cache.RemoveAsync(key, ct); }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Cache REMOVE failed: {Key}", key);
        }
    }
}
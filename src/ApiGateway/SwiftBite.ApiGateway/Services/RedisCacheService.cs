using Microsoft.Extensions.Caching.Distributed;
using System.Text;

namespace SwiftBite.ApiGateway.Services
{
    public class RedisCacheService: ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }
        public async Task<string?> GetAsync(string key)
        {
            try
            {
                var bytes = await _cache.GetAsync(key);
                if (bytes == null) return null;

                _logger.LogInformation("Cache HIT for key: {Key}", key);
                return Encoding.UTF8.GetString(bytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cache GET failed for key: {Key}", key);
                return null;
            }
        }
        public async Task SetAsync(string key, string value, TimeSpan? expiry = null)
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(5)
                };

                var bytes = Encoding.UTF8.GetBytes(value);
                await _cache.SetAsync(key, bytes, options);

                _logger.LogInformation("Cache SET for key: {Key}, Expiry: {Expiry}", key, expiry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cache SET failed for key: {Key}", key);
            }
        }
        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
                _logger.LogInformation("Cache REMOVE for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cache REMOVE failed for key: {Key}", key);
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            var value = await _cache.GetAsync(key);
            return value != null;
        }
    }
}

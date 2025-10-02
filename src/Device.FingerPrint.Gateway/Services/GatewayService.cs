using Microsoft.Extensions.Caching.Memory;

namespace Device.FingerPrint.Gateway.Services
{
    public interface IGatewayService
    {
        Task<bool> IsDisableDIAsync();
    }

    public class GatewayService : IGatewayService
    {
        private readonly IRedisService _redisService;
        private readonly IMemoryCache _memoryCache;
        
        private const string DisableDICacheKey = "disableDI_cache";
        private const string DisableGatewayRedisKey = "disableGateway";
        private const int CacheExpirationSeconds = 5;

        public GatewayService(IRedisService redisService, IMemoryCache memoryCache)
        {
            _redisService = redisService;
            _memoryCache = memoryCache;
        }

        public async Task<bool> IsDisableDIAsync()
        {
            // Try to get from memory cache first
            if (TryGetFromCache(out bool cachedValue))
            {
                return cachedValue;
            }

            try
            {
                var disableDIValue = await _redisService.GetAsync<string>(DisableGatewayRedisKey);
                var result = ParseDisableDIValue(disableDIValue);
                
                CacheResult(result);
                return result;
            }
            catch 
            { 
                return HandleErrorAndReturnDefault();
            }
        }

        private bool TryGetFromCache(out bool cachedValue)
        {
            return _memoryCache.TryGetValue(DisableDICacheKey, out cachedValue);
        }

        private static bool ParseDisableDIValue(string? disableDIValue)
        {
            // Mặc định trả về false nếu không tìm thấy value hoặc value là null/empty
            if (string.IsNullOrEmpty(disableDIValue))
            {
                return false;
            }
            
            // Chỉ trả về true nếu value là "true" (case insensitive)
            return disableDIValue.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        private void CacheResult(bool result)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CacheExpirationSeconds)
            };
            
            _memoryCache.Set(DisableDICacheKey, result, cacheOptions);
        }

        private bool HandleErrorAndReturnDefault()
        {
            // Cache the default value (false) for 5 seconds in case of error
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CacheExpirationSeconds)
            };
            
            _memoryCache.Set(DisableDICacheKey, false, cacheOptions);
            return false;
        }
    }
}

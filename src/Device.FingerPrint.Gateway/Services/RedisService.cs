using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Device.FingerPrint.Gateway.Services
{
    public interface IRedisService
    {
        IRedisDatabase RedisDatabase { get; }

        Task<bool> SetAsync<T>(string key, T value);

        Task<T?> GetAsync<T>(string key);

        Task<bool> AddAsyncAndExpiryAsync<T>(string key, T value, DateTimeOffset expiresAt);
    }

    public class RedisService : IRedisService
    {
        private const int MAX_RETRY_ATTEMPTS = 3;
        private const int RETRY_DELAY_MS = 1000;

        private readonly IRedisDatabase redisDatabase;
        private readonly ILogger _logger;

        public RedisService(IRedisClient redisClient, ILogger logger)
        {
            redisDatabase = redisClient.GetDefaultDatabase();
            _logger = logger;
        }

        public IRedisDatabase RedisDatabase => redisDatabase;

        public async Task<bool> SetAsync<T>(string key, T value)
        {
            return await ExecuteWithRetryAsync(async () =>
                await redisDatabase.SetAddAsync<T>(key, value),
                "Set");
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            return await ExecuteWithRetryAsync(async () =>
                await redisDatabase.GetAsync<T>(key),
                "Get");
        }

        private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, string operationName)
        {
            for (int attempt = 1; attempt <= MAX_RETRY_ATTEMPTS; attempt++)
            {
                try
                {
                    return await operation();
                }
                catch (RedisConnectionException ex)
                {
                    await _logger.WarnAsync("Redis connection error on attempt {Attempt} for operation {Operation}", attempt, operationName);

                    if (attempt == MAX_RETRY_ATTEMPTS)
                    {
                        await _logger.ErrorAsync($"Redis operation {operationName} failed after {MAX_RETRY_ATTEMPTS} attempts", ex);
                        throw;
                    }

                    await Task.Delay(RETRY_DELAY_MS * attempt);
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync($"Unexpected error in Redis operation {operationName}", ex);
                    throw;
                }
            }

            throw new InvalidOperationException($"Redis {operationName} failed after {MAX_RETRY_ATTEMPTS} attempts");
        }

        public async Task<bool> AddAsyncAndExpiryAsync<T>(string key, T value, DateTimeOffset expiresAt)
        {
            return await ExecuteWithRetryAsync(async () =>
                await redisDatabase.AddAsync(key, value, expiresAt),
                "AddWithExpiry");
        }
    }
}

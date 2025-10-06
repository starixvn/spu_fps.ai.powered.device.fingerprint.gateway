using Device.FingerPrint.Gateway.Models;
using System.Text.Json;

namespace Device.FingerPrint.Gateway.Services
{
    public interface ISubscriberService
    {
        Task<bool> ValidateAsync(string subscriber);
    }

    public class SubscriberService : ISubscriberService
    {
        private readonly ILogger logger;
        private readonly IAppSettings appSettings;
        private readonly IRedisService redisService;
        private readonly HttpClient httpClient;

        public SubscriberService(ILogger logger, IAppSettings appSettings, IRedisService redisService, HttpClient httpClient)
        {
            this.logger = logger;
            this.appSettings = appSettings;
            this.redisService = redisService;
            this.httpClient = httpClient;
        }

        public async Task<bool> ValidateAsync(string subscriber)
        {
            if (!string.IsNullOrWhiteSpace(subscriber))
            {
                var subscriberInfo = await GetSubscriberInfoAsync(subscriber);

                if (subscriberInfo != null && subscriberInfo.SubscriberId != 0)
                {
                    return true;
                }
            }

            return false;
        }

        private async Task<SubscriberModel> GetSubscriberInfoAsync(string subName)
        {

            if (!string.IsNullOrWhiteSpace(subName))
            {
                return await GetSubscriberInfo(subName);
            }

            return new SubscriberModel();
        }

        private async Task<SubscriberModel> GetSubscriberInfo(string subName)
        {
            if (!string.IsNullOrWhiteSpace(subName))
            {
                var cacheKey = $"GatewayAPI:{subName}";

                var subData = await GetSubscriberInfoByCache(cacheKey, subName);

                if (subData != null)
                {
                    return subData;
                }
                else
                {
                    subData = await GetDataSubscriberAsync(subName);

                    if (subData != null && subData.SubscriberId != 0)
                    {
                        _ = redisService.AddAsyncAndExpiryAsync(cacheKey, subData, DateTimeOffset.UtcNow.AddMinutes(120));

                        return subData;
                    }
                }
            }

            return new SubscriberModel();
        }

        private async Task<SubscriberModel?> GetSubscriberInfoByCache(string cacheKey, string subName)
        {
            try
            {
                return await redisService.GetAsync<SubscriberModel>(cacheKey);
            }
            catch (Exception)
            {
                return await GetDataSubscriberAndSetCacheAsync(subName, cacheKey);
            }
        }

        private async Task<SubscriberModel> GetDataSubscriberAndSetCacheAsync(string subName, string cacheKey)
        {
            var subData = await GetDataSubscriberAsync(subName);

            if (subData != null && subData.SubscriberId != 0)
            {
                var expiredAt = DateTimeOffset.UtcNow.AddMinutes(120);
                _ = redisService.AddAsyncAndExpiryAsync(cacheKey, subData, expiredAt);

                return subData;
            }

            return new SubscriberModel();
        }

        private async Task<SubscriberModel> GetDataSubscriberAsync(string subName)
        {
            try
            {
                var url = $"{appSettings.GetSubscriberInfo}?subscriberName={subName}";

                var response = await httpClient.GetAsync(url);
                var jsonString = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<SubscriberModel>(jsonString, options) ?? new SubscriberModel();
            }
            catch
            {
                logger.Error($"Cannot getSubscriber {subName} from {appSettings.GetSubscriberInfo}");
                return new SubscriberModel();
            }
        }
    }
}

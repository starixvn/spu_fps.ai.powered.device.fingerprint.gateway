using Device.FingerPrint.Gateway.Services;
using StackExchange.Redis;
using System.Net;

namespace Device.FingerPrint.Gateway.Middlewares
{
    public class RedisHealthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly IRedisService _redisService;

        public RedisHealthMiddleware(
            RequestDelegate next,
            ILogger logger,
            IRedisService redisService)
        {
            _next = next;
            _logger = logger;
            _redisService = redisService;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                if (!await IsRedisHealthyAsync())
                {
                    await _logger.WarnAsync("Redis connection is not healthy, but continuing with request processing");
                }

                await _next(httpContext);
            }
            catch (RedisConnectionException ex)
            {
                await _logger.ErrorAsync("Redis connection error occurred during request processing", ex);

                httpContext.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                await httpContext.Response.WriteAsync("Redis service temporarily unavailable");
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Unexpected error occurred during request processing", ex);
                throw;
            }
        }

        private async Task<bool> IsRedisHealthyAsync()
        {
            try
            {
                return await _redisService.SetAsync("health_check", DateTime.UtcNow.ToString());
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Redis health check failed", ex);
                return false;
            }
        }
    }
}

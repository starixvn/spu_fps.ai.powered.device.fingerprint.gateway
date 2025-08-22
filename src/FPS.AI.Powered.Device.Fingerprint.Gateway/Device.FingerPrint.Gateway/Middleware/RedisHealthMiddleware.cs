using Device.FingerPrint.Redis;
using Microsoft.AspNetCore.Http;
using System.Net;
using StackExchange.Redis;

namespace Device.FingerPrint.Gateway.Middleware
{
    public class RedisHealthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRedisService _redisService;
        private readonly ILogger<RedisHealthMiddleware> _logger;

        public RedisHealthMiddleware(RequestDelegate next, IRedisService redisService, ILogger<RedisHealthMiddleware> logger)
        {
            _next = next;
            _redisService = redisService;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Kiểm tra kết nối Redis trước khi xử lý request
                if (!await IsRedisHealthyAsync())
                {
                    _logger.LogWarning("Redis connection is not healthy, but continuing with request processing");
                }

                await _next(context);
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError(ex, "Redis connection error occurred during request processing");
                
                // Trả về lỗi 503 Service Unavailable nếu Redis không khả dụng
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                await context.Response.WriteAsync("Redis service temporarily unavailable");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred during request processing");
                throw;
            }
        }

        private async Task<bool> IsRedisHealthyAsync()
        {
            try
            {
                // Thực hiện một operation đơn giản để kiểm tra kết nối
                await _redisService.SetAsync("health_check", DateTime.UtcNow.ToString());
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis health check failed");
                return false;
            }
        }
    }
}

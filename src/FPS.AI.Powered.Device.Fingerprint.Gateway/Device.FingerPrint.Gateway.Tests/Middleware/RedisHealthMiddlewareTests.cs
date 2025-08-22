using Xunit;
using NSubstitute;
using Device.FingerPrint.Gateway.Middleware;
using Device.FingerPrint.Redis;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace Device.FingerPrint.Gateway.Tests.Middleware
{
    public class RedisHealthMiddlewareTests
    {
        [Fact]
        public async Task InvokeAsync_ShouldCallNext_WhenRedisIsHealthy()
        {
            // Arrange
            var next = Substitute.For<RequestDelegate>();
            var redisService = Substitute.For<IRedisService>();
            var logger = Substitute.For<ILogger<RedisHealthMiddleware>>();
            var context = new DefaultHttpContext();
            
            redisService.SetAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
            
            var middleware = new RedisHealthMiddleware(next, redisService, logger);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            await next.Received(1).Invoke(context);
        }

        [Fact]
        public async Task InvokeAsync_ShouldContinue_WhenRedisIsUnhealthy()
        {
            // Arrange
            var next = Substitute.For<RequestDelegate>();
            var redisService = Substitute.For<IRedisService>();
            var logger = Substitute.For<ILogger<RedisHealthMiddleware>>();
            var context = new DefaultHttpContext();
            
            redisService.SetAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromException<bool>(new Exception("Redis error")));
            
            var middleware = new RedisHealthMiddleware(next, redisService, logger);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            await next.Received(1).Invoke(context);
        }

        [Fact]
        public async Task InvokeAsync_ShouldReturn503_WhenRedisConnectionException()
        {
            // Arrange
            var next = Substitute.For<RequestDelegate>();
            var redisService = Substitute.For<IRedisService>();
            var logger = Substitute.For<ILogger<RedisHealthMiddleware>>();
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            
            next.When(x => x.Invoke(Arg.Any<HttpContext>()))
                .Do(x => throw new StackExchange.Redis.RedisConnectionException(StackExchange.Redis.ConnectionFailureType.SocketClosed, "Connection failed"));
            
            var middleware = new RedisHealthMiddleware(next, redisService, logger);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal((int)HttpStatusCode.ServiceUnavailable, context.Response.StatusCode);
        }
    }
}
using NSubstitute;
using Device.FingerPrint.Gateway.Middlewares;
using Microsoft.AspNetCore.Http;
using System.Net;
using Device.FingerPrint.Gateway.Services;

namespace Device.FingerPrint.Gateway.Tests.Middleware
{
    public class RedisHealthMiddlewareTests
    {
        private readonly ILogger logger;
        private readonly IRedisService redisService;

        public RedisHealthMiddlewareTests()
        {
            logger = Substitute.For<ILogger>();
            redisService = Substitute.For<IRedisService>();
        }

        [Fact]
        public async Task InvokeAsync_ShouldCallNext_WhenRedisIsHealthy()
        {
            // Arrange
            var next = Substitute.For<RequestDelegate>();
            var context = new DefaultHttpContext();
            
            redisService.SetAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
            
            var middleware = new RedisHealthMiddleware(next, logger, redisService);

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
            var context = new DefaultHttpContext();
            
            redisService.SetAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromException<bool>(new Exception("Redis error")));
            
            var middleware = new RedisHealthMiddleware(next, logger, redisService);

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
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            
            next.When(x => x.Invoke(Arg.Any<HttpContext>()))
                .Do(x => throw new StackExchange.Redis.RedisConnectionException(StackExchange.Redis.ConnectionFailureType.SocketClosed, "Connection failed"));
            
            var middleware = new RedisHealthMiddleware(next, logger, redisService);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal((int)HttpStatusCode.ServiceUnavailable, context.Response.StatusCode);
        }

        [Fact]
        public void RedisHealthMiddleware_ShouldBeCreated_WithValidDependencies()
        {
            // Arrange
            var next = Substitute.For<RequestDelegate>();

            // Act
            var middleware = new RedisHealthMiddleware(next, logger, redisService);

            // Assert
            Assert.NotNull(middleware);
        }
    }
}
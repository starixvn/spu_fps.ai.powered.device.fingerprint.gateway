using NSubstitute;
using Device.FingerPrint.Gateway.Services;
using Microsoft.Extensions.Caching.Memory;

namespace Device.FingerPrint.Gateway.Tests.Services
{
    public class GatewayServiceTests
    {
        private readonly IRedisService redisService;
        private readonly IMemoryCache memoryCache;

        public GatewayServiceTests()
        {
            redisService = Substitute.For<IRedisService>();
            memoryCache = Substitute.For<IMemoryCache>();
        }

        [Fact]
        public async Task IsDisableDIAsync_ReturnsFalse_WhenRedisReturnsNull()
        {
            // Arrange
            redisService.GetAsync<string>(Arg.Any<string>()).Returns(Task.FromResult<string>(null));
            memoryCache.TryGetValue(Arg.Any<string>(), out Arg.Any<object>()).Returns(false);
            
            var gatewayService = new GatewayService(redisService, memoryCache);

            // Act
            var result = await gatewayService.IsDisableDIAsync();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsDisableDIAsync_ReturnsTrue_WhenRedisReturnsTrue()
        {
            // Arrange
            redisService.GetAsync<string>(Arg.Any<string>()).Returns(Task.FromResult("true"));
            memoryCache.TryGetValue(Arg.Any<string>(), out Arg.Any<object>()).Returns(false);
            
            var gatewayService = new GatewayService(redisService, memoryCache);

            // Act
            var result = await gatewayService.IsDisableDIAsync();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsDisableDIAsync_ReturnsFalse_WhenRedisReturnsFalse()
        {
            // Arrange
            redisService.GetAsync<string>(Arg.Any<string>()).Returns(Task.FromResult("false"));
            memoryCache.TryGetValue(Arg.Any<string>(), out Arg.Any<object>()).Returns(false);
            
            var gatewayService = new GatewayService(redisService, memoryCache);

            // Act
            var result = await gatewayService.IsDisableDIAsync();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsDisableDIAsync_ReturnsCachedValue_WhenCacheHit()
        {
            // Arrange
            memoryCache.TryGetValue(Arg.Any<string>(), out Arg.Any<object>())
                .Returns(x => {
                    x[1] = true; // Set the out parameter
                    return true; // Return that cache hit occurred
                });
            
            var gatewayService = new GatewayService(redisService, memoryCache);

            // Act
            var result = await gatewayService.IsDisableDIAsync();

            // Assert
            Assert.True(result);
            await redisService.DidNotReceive().GetAsync<string>(Arg.Any<string>());
        }

        [Fact]
        public async Task IsDisableDIAsync_ReturnsFalse_WhenRedisThrowsException()
        {
            // Arrange
            redisService.GetAsync<string>(Arg.Any<string>()).Returns(Task.FromException<string>(new Exception("Redis error")));
            memoryCache.TryGetValue(Arg.Any<string>(), out Arg.Any<object>()).Returns(false);
            
            var gatewayService = new GatewayService(redisService, memoryCache);

            // Act
            var result = await gatewayService.IsDisableDIAsync();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsDisableDIAsync_IsCaseInsensitive_WhenRedisReturnsTrue()
        {
            // Arrange
            redisService.GetAsync<string>(Arg.Any<string>()).Returns(Task.FromResult("TRUE"));
            memoryCache.TryGetValue(Arg.Any<string>(), out Arg.Any<object>()).Returns(false);
            
            var gatewayService = new GatewayService(redisService, memoryCache);

            // Act
            var result = await gatewayService.IsDisableDIAsync();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GatewayService_ShouldBeCreated_WithValidDependencies()
        {
            // Arrange

            // Act
            var gatewayService = new GatewayService(redisService, memoryCache);

            // Assert
            Assert.NotNull(gatewayService);
        }
    }
}
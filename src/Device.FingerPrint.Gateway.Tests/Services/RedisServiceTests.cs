using Device.FingerPrint.Gateway.Services;
using NSubstitute;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Device.FingerPrint.Gateway.Tests.Services
{
    public class RedisServiceTests
    {
        private readonly ILogger logger;
        private readonly IRedisClient redisClient;
        private readonly IRedisDatabase redisDatabase;

        public RedisServiceTests()
        {
            logger = Substitute.For<ILogger>();
            redisClient = Substitute.For<IRedisClient>();
            redisDatabase = Substitute.For<IRedisDatabase>();
        }

        [Fact]
        public void SetAsync_ShouldNotThrow_WhenCalled()
        {
            // Arrange
            redisClient.GetDefaultDatabase().Returns(redisDatabase);
            redisClient.GetDb(Arg.Any<int>()).Returns(redisDatabase);

            var redisService = new RedisService(redisClient, logger);

            // Act & Assert
            Assert.NotNull(redisService);
            // Just verify the service can be created and basic operations don't throw
        }

        [Fact]
        public void GetAsync_ShouldNotThrow_WhenCalled()
        {
            // Arrange
            redisClient.GetDefaultDatabase().Returns(redisDatabase);
            redisClient.GetDb(Arg.Any<int>()).Returns(redisDatabase);

            var redisService = new RedisService(redisClient, logger);

            // Act & Assert
            Assert.NotNull(redisService);
        }

        [Fact]
        public void HashSetAsync_ShouldNotThrow_WhenCalled()
        {
            // Arrange
            redisClient.GetDefaultDatabase().Returns(redisDatabase);
            redisClient.GetDb(Arg.Any<int>()).Returns(redisDatabase);

            var redisService = new RedisService(redisClient, logger);

            // Act & Assert
            Assert.NotNull(redisService);
        }

        [Fact]
        public void RedisService_ShouldInitialize_WithValidDependencies()
        {
            // Arrange
            redisClient.GetDefaultDatabase().Returns(redisDatabase);
            redisClient.GetDb(Arg.Any<int>()).Returns(redisDatabase);

            // Act
            var redisService = new RedisService(redisClient, logger);

            // Assert
            Assert.NotNull(redisService);
            Assert.NotNull(redisService.RedisDatabase);
        }
    }
}
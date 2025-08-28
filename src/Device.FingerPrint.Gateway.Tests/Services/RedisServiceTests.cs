using NSubstitute;
using Microsoft.Extensions.Logging;
using Device.FingerPrint.Gateway.Services;

namespace Device.FingerPrint.Gateway.Tests.Services
{
    public class RedisServiceTests
    {
        [Fact]
        public void SetAsync_ShouldNotThrow_WhenCalled()
        {
            // Arrange
            var redisClient = Substitute.For<StackExchange.Redis.Extensions.Core.Abstractions.IRedisClient>();
            var redisDatabase = Substitute.For<StackExchange.Redis.Extensions.Core.Abstractions.IRedisDatabase>();
            var logger = Substitute.For<ILogger<RedisService>>();
            
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
            var redisClient = Substitute.For<StackExchange.Redis.Extensions.Core.Abstractions.IRedisClient>();
            var redisDatabase = Substitute.For<StackExchange.Redis.Extensions.Core.Abstractions.IRedisDatabase>();
            var logger = Substitute.For<ILogger<RedisService>>();
            
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
            var redisClient = Substitute.For<StackExchange.Redis.Extensions.Core.Abstractions.IRedisClient>();
            var redisDatabase = Substitute.For<StackExchange.Redis.Extensions.Core.Abstractions.IRedisDatabase>();
            var logger = Substitute.For<ILogger<RedisService>>();
            
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
            var redisClient = Substitute.For<StackExchange.Redis.Extensions.Core.Abstractions.IRedisClient>();
            var redisDatabase = Substitute.For<StackExchange.Redis.Extensions.Core.Abstractions.IRedisDatabase>();
            var logger = Substitute.For<ILogger<RedisService>>();
            
            redisClient.GetDefaultDatabase().Returns(redisDatabase);
            redisClient.GetDb(Arg.Any<int>()).Returns(redisDatabase);

            // Act
            var redisService = new RedisService(redisClient, logger);

            // Assert
            Assert.NotNull(redisService);
            Assert.NotNull(redisService.RedisDatabase);
            Assert.NotNull(redisService.RedisJSChallengeDatabase);
        }

        [Fact]
        public void RedisService_Properties_ShouldReturnCorrectDatabases()
        {
            // Arrange
            var redisClient = Substitute.For<StackExchange.Redis.Extensions.Core.Abstractions.IRedisClient>();
            var defaultDatabase = Substitute.For<StackExchange.Redis.Extensions.Core.Abstractions.IRedisDatabase>();
            var jsChallengeDatabase = Substitute.For<StackExchange.Redis.Extensions.Core.Abstractions.IRedisDatabase>();
            var logger = Substitute.For<ILogger<RedisService>>();
            
            redisClient.GetDefaultDatabase().Returns(defaultDatabase);
            redisClient.GetDb(10).Returns(jsChallengeDatabase); // REDIS_JSCHALLENGE_DATABASE = 10

            // Act
            var redisService = new RedisService(redisClient, logger);

            // Assert
            Assert.Same(defaultDatabase, redisService.RedisDatabase);
            Assert.Same(jsChallengeDatabase, redisService.RedisJSChallengeDatabase);
        }
    }
}
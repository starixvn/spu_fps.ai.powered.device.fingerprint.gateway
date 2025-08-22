using Xunit;
using NSubstitute;
using Device.FingerPrint.Gateway.Services;
using Device.FingerPrint.Redis;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Device.FingerPrint.Gateway.Tests.Services
{
    public class RedisServiceTests
    {
        [Fact]
        public async Task SetAsync_ShouldReturnTrue_WhenOperationSucceeds()
        {
            // Arrange
            var redisClient = Substitute.For<StackExchange.Redis.Extensions.Core.Abstractions.IRedisClient>();
            var redisDatabase = Substitute.For<StackExchange.Redis.Extensions.Core.Abstractions.IRedisDatabase>();
            var logger = Substitute.For<ILogger<RedisService>>();
            
            redisClient.GetDefaultDatabase().Returns(redisDatabase);
            redisClient.GetDb(Arg.Any<int>()).Returns(redisDatabase);
            redisDatabase.AddAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

            var redisService = new RedisService(redisClient, logger);

            // Act
            var result = await redisService.SetAsync("test_key", "test_value");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnValue_WhenKeyExists()
        {
            // Arrange
            var redisClient = Substitute.For<StackExchange.Redis.Extensions.Core.Abstractions.IRedisClient>();
            var redisDatabase = Substitute.For<StackExchange.Redis.Extensions.Core.Abstractions.IRedisDatabase>();
            var logger = Substitute.For<ILogger<RedisService>>();
            
            redisClient.GetDefaultDatabase().Returns(redisDatabase);
            redisClient.GetDb(Arg.Any<int>()).Returns(redisDatabase);
            redisDatabase.GetAsync<string>(Arg.Any<string>()).Returns("test_value");

            var redisService = new RedisService(redisClient, logger);

            // Act
            var result = await redisService.GetAsync<string>("test_key");

            // Assert
            Assert.Equal("test_value", result);
        }

        [Fact]
        public async Task HashSetAsync_ShouldReturnTrue_WhenOperationSucceeds()
        {
            // Arrange
            var redisClient = Substitute.For<StackExchange.Redis.Extensions.Core.Abstractions.IRedisClient>();
            var redisDatabase = Substitute.For<StackExchange.Redis.Extensions.Core.Abstractions.IRedisDatabase>();
            var logger = Substitute.For<ILogger<RedisService>>();
            
            redisClient.GetDefaultDatabase().Returns(redisDatabase);
            redisClient.GetDb(Arg.Any<int>()).Returns(redisDatabase);
            redisDatabase.HashSetAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(true);

            var redisService = new RedisService(redisClient, logger);

            // Act
            var result = await redisService.HashSetAsync("hash_key", "field", "value", false);

            // Assert
            Assert.True(result);
        }
    }
}
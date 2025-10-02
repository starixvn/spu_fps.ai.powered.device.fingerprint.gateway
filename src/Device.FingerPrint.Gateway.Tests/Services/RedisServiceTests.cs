using Device.FingerPrint.Gateway.Services;
using NSubstitute;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis;

namespace Device.FingerPrint.Gateway.Tests.Services
{
    public class RedisServiceTests
    {
        private readonly ILogger mockLogger;
        private readonly IRedisClient mockRedisClient;
        private readonly IRedisDatabase mockRedisDatabase;
        private readonly RedisService redisService;

        public RedisServiceTests()
        {
            mockLogger = Substitute.For<ILogger>();
            mockRedisClient = Substitute.For<IRedisClient>();
            mockRedisDatabase = Substitute.For<IRedisDatabase>();
            
            mockRedisClient.GetDefaultDatabase().Returns(mockRedisDatabase);
            
            redisService = new RedisService(mockRedisClient, mockLogger);
        }

        #region SetAsync Tests

        [Fact]
        public async Task SetAsync_ShouldReturnTrue_WhenOperationSucceeds()
        {
            // Arrange
            const string key = "test-key";
            const string value = "test-value";
            mockRedisDatabase.SetAddAsync<string>(key, value, flag: CommandFlags.FireAndForget)
                .Returns(true);

            // Act
            var result = await redisService.SetAsync(key, value);

            // Assert
            Assert.True(result);
            await mockRedisDatabase.Received(1).SetAddAsync<string>(key, value, flag: CommandFlags.FireAndForget);
        }

        [Fact]
        public async Task SetAsync_ShouldReturnFalse_WhenOperationFails()
        {
            // Arrange
            const string key = "test-key";
            const string value = "test-value";
            mockRedisDatabase.SetAddAsync<string>(key, value, flag: CommandFlags.FireAndForget)
                .Returns(false);

            // Act
            var result = await redisService.SetAsync(key, value);

            // Assert
            Assert.False(result);
            await mockRedisDatabase.Received(1).SetAddAsync<string>(key, value, flag: CommandFlags.FireAndForget);
        }

        [Fact]
        public async Task SetAsync_ShouldRetryAndSucceed_OnRedisConnectionException()
        {
            // Arrange
            const string key = "test-key";
            const string value = "test-value";
            var connectionException = new RedisConnectionException(ConnectionFailureType.SocketClosed, "Connection failed");
            
            mockRedisDatabase.SetAddAsync<string>(key, value, flag: CommandFlags.FireAndForget)
                .Returns(Task.FromException<bool>(connectionException), Task.FromResult(true));

            // Act
            var result = await redisService.SetAsync(key, value);

            // Assert
            Assert.True(result);
            await mockRedisDatabase.Received(2).SetAddAsync<string>(key, value, flag: CommandFlags.FireAndForget);
            await mockLogger.Received(1).WarnAsync("Redis connection error on attempt {Attempt} for operation {Operation}", 1, "Set");
        }

        [Fact]
        public async Task SetAsync_ShouldThrowAfterMaxRetries_OnRedisConnectionException()
        {
            // Arrange
            const string key = "test-key";
            const string value = "test-value";
            var connectionException = new RedisConnectionException(ConnectionFailureType.SocketClosed, "Connection failed");
            
            mockRedisDatabase.SetAddAsync<string>(key, value, flag: CommandFlags.FireAndForget)
                .Returns(Task.FromException<bool>(connectionException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<RedisConnectionException>(() => redisService.SetAsync(key, value));
            Assert.Equal(connectionException, exception);
            
            await mockRedisDatabase.Received(3).SetAddAsync<string>(key, value, flag: CommandFlags.FireAndForget);
            await mockLogger.Received(3).WarnAsync("Redis connection error on attempt {Attempt} for operation {Operation}", Arg.Any<int>(), "Set");
            await mockLogger.Received(1).ErrorAsync("Redis operation Set failed after 3 attempts", connectionException);
        }

        [Fact]
        public async Task SetAsync_ShouldThrowImmediately_OnUnexpectedException()
        {
            // Arrange
            const string key = "test-key";
            const string value = "test-value";
            var unexpectedException = new InvalidOperationException("Unexpected error");
            
            mockRedisDatabase.SetAddAsync<string>(key, value, flag: CommandFlags.FireAndForget)
                .Returns(Task.FromException<bool>(unexpectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => redisService.SetAsync(key, value));
            Assert.Equal(unexpectedException, exception);
            
            await mockRedisDatabase.Received(1).SetAddAsync<string>(key, value, flag: CommandFlags.FireAndForget);
            await mockLogger.Received(1).ErrorAsync("Unexpected error in Redis operation Set", unexpectedException);
        }

        [Fact]
        public async Task SetAsync_ShouldHandleComplexObject()
        {
            // Arrange
            const string key = "test-key";
            var complexValue = new { Id = 1, Name = "Test", Items = new[] { "a", "b", "c" } };
            mockRedisDatabase.SetAddAsync(key, complexValue, flag: CommandFlags.FireAndForget)
                .Returns(true);

            // Act
            var result = await redisService.SetAsync(key, complexValue);

            // Assert
            Assert.True(result);
            await mockRedisDatabase.Received(1).SetAddAsync(key, complexValue, flag: CommandFlags.FireAndForget);
        }

        #endregion

        #region GetAsync Tests

        [Fact]
        public async Task GetAsync_ShouldReturnValue_WhenKeyExists()
        {
            // Arrange
            const string key = "test-key";
            const string expectedValue = "test-value";
            mockRedisDatabase.GetAsync<string>(key).Returns(expectedValue);

            // Act
            var result = await redisService.GetAsync<string>(key);

            // Assert
            Assert.Equal(expectedValue, result);
            await mockRedisDatabase.Received(1).GetAsync<string>(key);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnNull_WhenKeyDoesNotExist()
        {
            // Arrange
            const string key = "test-key";
            mockRedisDatabase.GetAsync<string>(key).Returns(Task.FromResult<string>(null));

            // Act
            var result = await redisService.GetAsync<string>(key);

            // Assert
            Assert.Null(result);
            await mockRedisDatabase.Received(1).GetAsync<string>(key);
        }

        [Fact]
        public async Task GetAsync_ShouldRetryAndSucceed_OnRedisConnectionException()
        {
            // Arrange
            const string key = "test-key";
            const string expectedValue = "test-value";
            var connectionException = new RedisConnectionException(ConnectionFailureType.SocketClosed, "Connection failed");
            
            mockRedisDatabase.GetAsync<string>(key)
                .Returns(Task.FromException<string>(connectionException), Task.FromResult(expectedValue));

            // Act
            var result = await redisService.GetAsync<string>(key);

            // Assert
            Assert.Equal(expectedValue, result);
            await mockRedisDatabase.Received(2).GetAsync<string>(key);
            await mockLogger.Received(1).WarnAsync("Redis connection error on attempt {Attempt} for operation {Operation}", 1, "Get");
        }

        [Fact]
        public async Task GetAsync_ShouldThrowAfterMaxRetries_OnRedisConnectionException()
        {
            // Arrange
            const string key = "test-key";
            var connectionException = new RedisConnectionException(ConnectionFailureType.SocketClosed, "Connection failed");
            
            mockRedisDatabase.GetAsync<string>(key)
                .Returns(Task.FromException<string>(connectionException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<RedisConnectionException>(() => redisService.GetAsync<string>(key));
            Assert.Equal(connectionException, exception);
            
            await mockRedisDatabase.Received(3).GetAsync<string>(key);
            await mockLogger.Received(3).WarnAsync("Redis connection error on attempt {Attempt} for operation {Operation}", Arg.Any<int>(), "Get");
            await mockLogger.Received(1).ErrorAsync("Redis operation Get failed after 3 attempts", connectionException);
        }

        [Fact]
        public async Task GetAsync_ShouldHandleComplexObject()
        {
            // Arrange
            const string key = "test-key";
            var expectedValue = new { Id = 1, Name = "Test", Items = new[] { "a", "b", "c" } };
            mockRedisDatabase.GetAsync<object>(key).Returns(expectedValue);

            // Act
            var result = await redisService.GetAsync<object>(key);

            // Assert
            Assert.Equal(expectedValue, result);
            await mockRedisDatabase.Received(1).GetAsync<object>(key);
        }

        #endregion

        #region AddAsyncAndExpiryAsync Tests

        [Fact]
        public async Task AddAsyncAndExpiryAsync_ShouldReturnTrue_WhenOperationSucceeds()
        {
            // Arrange
            const string key = "test-key";
            const string value = "test-value";
            var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
            mockRedisDatabase.AddAsync(key, value, expiresAt, flag: CommandFlags.FireAndForget)
                .Returns(true);

            // Act
            var result = await redisService.AddAsyncAndExpiryAsync(key, value, expiresAt);

            // Assert
            Assert.True(result);
            await mockRedisDatabase.Received(1).AddAsync(key, value, expiresAt, flag: CommandFlags.FireAndForget);
        }

        [Fact]
        public async Task AddAsyncAndExpiryAsync_ShouldReturnFalse_WhenOperationFails()
        {
            // Arrange
            const string key = "test-key";
            const string value = "test-value";
            var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
            mockRedisDatabase.AddAsync(key, value, expiresAt, flag: CommandFlags.FireAndForget)
                .Returns(false);

            // Act
            var result = await redisService.AddAsyncAndExpiryAsync(key, value, expiresAt);

            // Assert
            Assert.False(result);
            await mockRedisDatabase.Received(1).AddAsync(key, value, expiresAt, flag: CommandFlags.FireAndForget);
        }

        [Fact]
        public async Task AddAsyncAndExpiryAsync_ShouldRetryAndSucceed_OnRedisConnectionException()
        {
            // Arrange
            const string key = "test-key";
            const string value = "test-value";
            var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
            var connectionException = new RedisConnectionException(ConnectionFailureType.SocketClosed, "Connection failed");
            
            mockRedisDatabase.AddAsync(key, value, expiresAt, flag: CommandFlags.FireAndForget)
                .Returns(Task.FromException<bool>(connectionException), Task.FromResult(true));

            // Act
            var result = await redisService.AddAsyncAndExpiryAsync(key, value, expiresAt);

            // Assert
            Assert.True(result);
            await mockRedisDatabase.Received(2).AddAsync(key, value, expiresAt, flag: CommandFlags.FireAndForget);
            await mockLogger.Received(1).WarnAsync("Redis connection error on attempt {Attempt} for operation {Operation}", 1, "AddWithExpiry");
        }

        [Fact]
        public async Task AddAsyncAndExpiryAsync_ShouldThrowAfterMaxRetries_OnRedisConnectionException()
        {
            // Arrange
            const string key = "test-key";
            const string value = "test-value";
            var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
            var connectionException = new RedisConnectionException(ConnectionFailureType.SocketClosed, "Connection failed");
            
            mockRedisDatabase.AddAsync(key, value, expiresAt, flag: CommandFlags.FireAndForget)
                .Returns(Task.FromException<bool>(connectionException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<RedisConnectionException>(() => redisService.AddAsyncAndExpiryAsync(key, value, expiresAt));
            Assert.Equal(connectionException, exception);
            
            await mockRedisDatabase.Received(3).AddAsync(key, value, expiresAt, flag: CommandFlags.FireAndForget);
            await mockLogger.Received(3).WarnAsync("Redis connection error on attempt {Attempt} for operation {Operation}", Arg.Any<int>(), "AddWithExpiry");
            await mockLogger.Received(1).ErrorAsync("Redis operation AddWithExpiry failed after 3 attempts", connectionException);
        }

        [Fact]
        public async Task AddAsyncAndExpiryAsync_ShouldHandleExpiredDate()
        {
            // Arrange
            const string key = "test-key";
            const string value = "test-value";
            var expiresAt = DateTimeOffset.UtcNow.AddDays(-1); // Expired date
            mockRedisDatabase.AddAsync(key, value, expiresAt, flag: CommandFlags.FireAndForget)
                .Returns(false);

            // Act
            var result = await redisService.AddAsyncAndExpiryAsync(key, value, expiresAt);

            // Assert
            Assert.False(result);
            await mockRedisDatabase.Received(1).AddAsync(key, value, expiresAt, flag: CommandFlags.FireAndForget);
        }

        [Fact]
        public async Task AddAsyncAndExpiryAsync_ShouldHandleComplexObject()
        {
            // Arrange
            const string key = "test-key";
            var complexValue = new { Id = 1, Name = "Test", Items = new[] { "a", "b", "c" } };
            var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
            mockRedisDatabase.AddAsync(key, complexValue, expiresAt, flag: CommandFlags.FireAndForget)
                .Returns(true);

            // Act
            var result = await redisService.AddAsyncAndExpiryAsync(key, complexValue, expiresAt);

            // Assert
            Assert.True(result);
            await mockRedisDatabase.Received(1).AddAsync(key, complexValue, expiresAt, flag: CommandFlags.FireAndForget);
        }

        #endregion

        #region Retry Logic Tests

        [Fact]
        public async Task ExecuteWithRetryAsync_ShouldRespectRetryDelay()
        {
            // Arrange
            const string key = "test-key";
            const string value = "test-value";
            var connectionException = new RedisConnectionException(ConnectionFailureType.SocketClosed, "Connection failed");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            mockRedisDatabase.SetAddAsync<string>(key, value, flag: CommandFlags.FireAndForget)
                .Returns(Task.FromException<bool>(connectionException), Task.FromResult(true));

            // Act
            await redisService.SetAsync(key, value);
            stopwatch.Stop();

            // Assert - Should have waited at least 1000ms (RETRY_DELAY_MS * attempt = 1000ms * 1)
            Assert.True(stopwatch.ElapsedMilliseconds >= 1000);
        }

        [Fact]
        public async Task ExecuteWithRetryAsync_ShouldLogCorrectAttemptNumbers()
        {
            // Arrange
            const string key = "test-key";
            const string value = "test-value";
            var connectionException = new RedisConnectionException(ConnectionFailureType.SocketClosed, "Connection failed");
            
            mockRedisDatabase.SetAddAsync<string>(key, value, flag: CommandFlags.FireAndForget)
                .Returns(Task.FromException<bool>(connectionException));

            // Act & Assert
            await Assert.ThrowsAsync<RedisConnectionException>(() => redisService.SetAsync(key, value));
            
            // Verify correct attempt numbers were logged
            await mockLogger.Received(1).WarnAsync("Redis connection error on attempt {Attempt} for operation {Operation}", 1, "Set");
            await mockLogger.Received(1).WarnAsync("Redis connection error on attempt {Attempt} for operation {Operation}", 2, "Set");
            await mockLogger.Received(1).WarnAsync("Redis connection error on attempt {Attempt} for operation {Operation}", 3, "Set");
        }

        #endregion

        #region Multiple Operations Tests

        [Fact]
        public async Task MultipleOperations_ShouldWorkIndependently()
        {
            // Arrange
            const string key1 = "key1";
            const string key2 = "key2";
            const string value1 = "value1";
            const string value2 = "value2";
            
            mockRedisDatabase.SetAddAsync<string>(key1, value1, flag: CommandFlags.FireAndForget).Returns(true);
            mockRedisDatabase.GetAsync<string>(key2).Returns(value2);

            // Act
            var setResult = await redisService.SetAsync(key1, value1);
            var getResult = await redisService.GetAsync<string>(key2);

            // Assert
            Assert.True(setResult);
            Assert.Equal(value2, getResult);
            await mockRedisDatabase.Received(1).SetAddAsync<string>(key1, value1, flag: CommandFlags.FireAndForget);
            await mockRedisDatabase.Received(1).GetAsync<string>(key2);
        }

        #endregion
    }
}
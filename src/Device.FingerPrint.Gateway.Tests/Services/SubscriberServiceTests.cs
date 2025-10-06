using NSubstitute;
using Device.FingerPrint.Gateway.Services;
using Device.FingerPrint.Gateway.Models;

namespace Device.FingerPrint.Gateway.Tests.Services
{
    public class SubscriberServiceTests
    {
        private readonly ILogger logger;
        private readonly IAppSettings appSettings;
        private readonly IRedisService redisService;
        private readonly HttpClient httpClient;

        public SubscriberServiceTests()
        {
            logger = Substitute.For<ILogger>();
            appSettings = Substitute.For<IAppSettings>();
            redisService = Substitute.For<IRedisService>();
            httpClient = Substitute.For<HttpClient>();
        }

        [Fact]
        public async Task ValidateAsync_ReturnsTrue_WhenSubscriberIsValid()
        {
            // Arrange
            var subscriberModel = new SubscriberModel 
            { 
                SubscriberId = 123, 
                SubscriberName = "TestSubscriber" 
            };
            
            redisService.GetAsync<SubscriberModel>(Arg.Any<string>()).Returns(subscriberModel);
            
            var subscriberService = new SubscriberService(logger, appSettings, redisService, httpClient);

            // Act
            var result = await subscriberService.ValidateAsync("ValidSubscriber");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateAsync_ReturnsFalse_WhenSubscriberIsNull()
        {
            // Arrange
            var subscriberService = new SubscriberService(logger, appSettings, redisService, httpClient);

            // Act
            var result = await subscriberService.ValidateAsync(null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateAsync_ReturnsFalse_WhenSubscriberIsEmpty()
        {
            // Arrange
            var subscriberService = new SubscriberService(logger, appSettings, redisService, httpClient);

            // Act
            var result = await subscriberService.ValidateAsync("");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateAsync_ReturnsFalse_WhenSubscriberIsWhitespace()
        {
            // Arrange
            var subscriberService = new SubscriberService(logger, appSettings, redisService, httpClient);

            // Act
            var result = await subscriberService.ValidateAsync("   ");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateAsync_ReturnsFalse_WhenSubscriberIdIsZero()
        {
            // Arrange
            var subscriberModel = new SubscriberModel 
            { 
                SubscriberId = 0, 
                SubscriberName = "TestSubscriber" 
            };
            
            redisService.GetAsync<SubscriberModel>(Arg.Any<string>()).Returns(subscriberModel);
            
            var subscriberService = new SubscriberService(logger, appSettings, redisService, httpClient);

            // Act
            var result = await subscriberService.ValidateAsync("InvalidSubscriber");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateAsync_ReturnsFalse_WhenSubscriberModelIsNull()
        {
            // Arrange
            redisService.GetAsync<SubscriberModel>(Arg.Any<string>()).Returns(Task.FromResult<SubscriberModel>(null));

            var subscriberService = new SubscriberService(logger, appSettings, redisService, httpClient);

            // Act
            var result = await subscriberService.ValidateAsync("NonExistentSubscriber");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void SubscriberService_ShouldBeCreated_WithValidDependencies()
        {
            // Arrange

            // Act
            var subscriberService = new SubscriberService(logger, appSettings, redisService, httpClient);

            // Assert
            Assert.NotNull(subscriberService);
        }
    }
}
using NSubstitute;
using Device.FingerPrint.Gateway.Services;
using Device.FingerPrint.Gateway.Models;

namespace Device.FingerPrint.Gateway.Tests.Services
{
    public class SubscriberServiceTests
    {
        [Fact]
        public async Task ValidateAsync_ReturnsTrue_WhenSubscriberIsValid()
        {
            // Arrange
            var appSettings = Substitute.For<IAppSettings>();
            var redisService = Substitute.For<IRedisService>();
            
            var subscriberModel = new SubscriberModel 
            { 
                SubscriberId = 123, 
                SubscriberName = "TestSubscriber" 
            };
            
            redisService.GetAsync<SubscriberModel>(Arg.Any<string>()).Returns(subscriberModel);
            
            var subscriberService = new SubscriberService(appSettings, redisService);

            // Act
            var result = await subscriberService.ValidateAsync("ValidSubscriber");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateAsync_ReturnsFalse_WhenSubscriberIsNull()
        {
            // Arrange
            var appSettings = Substitute.For<IAppSettings>();
            var redisService = Substitute.For<IRedisService>();
            
            var subscriberService = new SubscriberService(appSettings, redisService);

            // Act
            var result = await subscriberService.ValidateAsync(null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateAsync_ReturnsFalse_WhenSubscriberIsEmpty()
        {
            // Arrange
            var appSettings = Substitute.For<IAppSettings>();
            var redisService = Substitute.For<IRedisService>();
            
            var subscriberService = new SubscriberService(appSettings, redisService);

            // Act
            var result = await subscriberService.ValidateAsync("");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateAsync_ReturnsFalse_WhenSubscriberIsWhitespace()
        {
            // Arrange
            var appSettings = Substitute.For<IAppSettings>();
            var redisService = Substitute.For<IRedisService>();
            
            var subscriberService = new SubscriberService(appSettings, redisService);

            // Act
            var result = await subscriberService.ValidateAsync("   ");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateAsync_ReturnsFalse_WhenSubscriberIdIsZero()
        {
            // Arrange
            var appSettings = Substitute.For<IAppSettings>();
            var redisService = Substitute.For<IRedisService>();
            
            var subscriberModel = new SubscriberModel 
            { 
                SubscriberId = 0, 
                SubscriberName = "TestSubscriber" 
            };
            
            redisService.GetAsync<SubscriberModel>(Arg.Any<string>()).Returns(subscriberModel);
            
            var subscriberService = new SubscriberService(appSettings, redisService);

            // Act
            var result = await subscriberService.ValidateAsync("InvalidSubscriber");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateAsync_ReturnsFalse_WhenSubscriberModelIsNull()
        {
            // Arrange
            var appSettings = Substitute.For<IAppSettings>();
            var redisService = Substitute.For<IRedisService>();
            
            redisService.GetAsync<SubscriberModel>(Arg.Any<string>()).Returns(Task.FromResult<SubscriberModel>(null));
            
            var subscriberService = new SubscriberService(appSettings, redisService);

            // Act
            var result = await subscriberService.ValidateAsync("NonExistentSubscriber");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void SubscriberService_ShouldBeCreated_WithValidDependencies()
        {
            // Arrange
            var appSettings = Substitute.For<IAppSettings>();
            var redisService = Substitute.For<IRedisService>();

            // Act
            var subscriberService = new SubscriberService(appSettings, redisService);

            // Assert
            Assert.NotNull(subscriberService);
        }
    }
}
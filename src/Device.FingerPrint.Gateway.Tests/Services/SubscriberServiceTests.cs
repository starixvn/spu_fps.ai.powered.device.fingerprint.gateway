using NSubstitute;
using Device.FingerPrint.Gateway.Services;
using Device.FingerPrint.Gateway.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Http;

namespace Device.FingerPrint.Gateway.Tests.Services
{
    public class SubscriberServiceTests
    {
        private readonly ILogger logger;
        private readonly IAppSettings appSettings;
        private readonly IRedisService redisService;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly SubscriberService sut; // Add System Under Test

        public SubscriberServiceTests()
        {
            logger = Substitute.For<ILogger>();
            appSettings = Substitute.For<IAppSettings>();
            redisService = Substitute.For<IRedisService>();
            httpClientFactory = Substitute.For<IHttpClientFactory>();
            sut = new SubscriberService(logger, appSettings, redisService, httpClientFactory); // Initialize once
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ValidateAsync_ReturnsFalse_ForInvalidSubscriberInput(string subscriberInput)
        {
            // Act
            var result = await sut.ValidateAsync(subscriberInput);

            // Assert
            Assert.False(result);
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

            // Act
            var result = await sut.ValidateAsync("ValidSubscriber");

            // Assert
            Assert.True(result);
            await redisService.Received(1).GetAsync<SubscriberModel>(Arg.Any<string>());
        }

        [Theory]
        [InlineData(0)]  // Invalid ID
        public async Task ValidateAsync_ReturnsFalse_WhenSubscriberModelIsInvalid(int subscriberId)
        {
            // Arrange
            var subscriberModel = new SubscriberModel 
            { 
                SubscriberId = subscriberId, 
                SubscriberName = "TestSubscriber" 
            };
            
            redisService.GetAsync<SubscriberModel>(Arg.Any<string>()).Returns(subscriberModel);

            // Act
            var result = await sut.ValidateAsync("InvalidSubscriber");

            // Assert
            Assert.False(result);
            await redisService.Received(1).GetAsync<SubscriberModel>(Arg.Any<string>());
        }

        [Fact]
        public async Task ValidateAsync_ReturnsFalse_WhenSubscriberModelIsNull()
        {
            // Arrange
            redisService.GetAsync<SubscriberModel>(Arg.Any<string>()).Returns(Task.FromResult<SubscriberModel>(null));

            // Act
            var result = await sut.ValidateAsync("NonExistentSubscriber");

            // Assert
            Assert.False(result);
            await redisService.Received(1).GetAsync<SubscriberModel>(Arg.Any<string>());
        }
    }
}
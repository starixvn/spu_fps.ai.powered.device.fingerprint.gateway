using NSubstitute;
using Device.FingerPrint.Gateway.Handlers;
using Device.FingerPrint.Gateway.Services;
using System.Text;
using System.Reflection;

namespace Device.FingerPrint.Gateway.Tests.Handlers
{
    public class MobileRequestHandlerTests
    {
        private async Task<HttpResponseMessage> CallSendAsync(MobileRequestHandler handler, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Use reflection to call protected SendAsync method
            var method = typeof(MobileRequestHandler).GetMethod("SendAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            var task = (Task<HttpResponseMessage>)method.Invoke(handler, new object[] { request, cancellationToken });
            return await task;
        }

        [Fact]
        public async Task SendAsync_ShouldProcessMobileRequest_WhenValidContent()
        {
            // Arrange
            var subscriberService = Substitute.For<ISubscriberService>();
            var gatewayService = Substitute.For<IGatewayService>();
            var logger = Substitute.For<ILogger>();
            var appSettings = Substitute.For<IAppSettings>();

            var handler = new MobileRequestHandler(logger, subscriberService, gatewayService, appSettings);
            var request = new HttpRequestMessage();
            request.Content = new StringContent("{\"Transaction\":{\"DeviceId\":\"test123\"}}", Encoding.UTF8, "application/json");

            // Act
            var response = await CallSendAsync(handler, request, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
        }

        [Fact]
        public async Task SendAsync_ShouldHandleEmptyContent_Gracefully()
        {
            // Arrange
            var subscriberService = Substitute.For<ISubscriberService>();
            var gatewayService = Substitute.For<IGatewayService>();
            var logger = Substitute.For<ILogger>();
            var appSettings = Substitute.For<IAppSettings>();

            var handler = new MobileRequestHandler(logger, subscriberService, gatewayService, appSettings);
            var request = new HttpRequestMessage();
            request.Content = new StringContent("", Encoding.UTF8, "application/json");

            // Act
            var response = await CallSendAsync(handler, request, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
        }

        [Fact]
        public async Task SendAsync_ShouldHandleNullContent_Gracefully()
        {
            // Arrange
            var subscriberService = Substitute.For<ISubscriberService>();
            var gatewayService = Substitute.For<IGatewayService>();
            var logger = Substitute.For<ILogger>();
            var appSettings = Substitute.For<IAppSettings>();

            var handler = new MobileRequestHandler(logger, subscriberService, gatewayService, appSettings);
            var request = new HttpRequestMessage();
            request.Content = null;

            // Act
            var response = await CallSendAsync(handler, request, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
        }

        [Fact]
        public async Task SendAsync_ShouldHandleInvalidJson_Gracefully()
        {
            // Arrange
            var subscriberService = Substitute.For<ISubscriberService>();
            var gatewayService = Substitute.For<IGatewayService>();
            var logger = Substitute.For<ILogger>();
            var appSettings = Substitute.For<IAppSettings>();

            var handler = new MobileRequestHandler(logger, subscriberService, gatewayService, appSettings);
            var request = new HttpRequestMessage();
            request.Content = new StringContent("{invalid json}", Encoding.UTF8, "application/json");

            // Act
            var response = await CallSendAsync(handler, request, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
        }

        [Fact]
        public void MobileRequestHandler_ShouldBeCreated_WithValidDependencies()
        {
            // Arrange
            var subscriberService = Substitute.For<ISubscriberService>();
            var gatewayService = Substitute.For<IGatewayService>();
            var logger = Substitute.For<ILogger>();
            var appSettings = Substitute.For<IAppSettings>();

            // Act
            var handler = new MobileRequestHandler(logger, subscriberService, gatewayService, appSettings);

            // Assert
            Assert.NotNull(handler);
        }
    }
}
using Xunit;
using NSubstitute;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Device.FingerPrint.Gateway.Handlers;
using Device.FingerPrint.Gateway.Services;
using Device.FingerPrint.Gateway.Model;
using Newtonsoft.Json;
using System.Text;
using System.Reflection;

namespace Device.FingerPrint.Gateway.Tests.Handlers
{
    public class ValidateHandlerTests
    {
        private async Task<HttpResponseMessage> CallSendAsync(ValidateHandler handler, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Use reflection to call protected SendAsync method
            var method = typeof(ValidateHandler).GetMethod("SendAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            var task = (Task<HttpResponseMessage>)method.Invoke(handler, new object[] { request, cancellationToken });
            return await task;
        }

        [Fact]
        public async Task SendAsync_ReturnsBadRequest_WhenRequestIsNull()
        {
            // Arrange
            var subscriberService = Substitute.For<ISubscriberService>();
            var gatewayService = Substitute.For<IGatewayService>();
            gatewayService.IsDisableDIAsync().Returns(false);
            
            var handler = new ValidateHandler(subscriberService, gatewayService);
            
            var request = new HttpRequestMessage();
            request.Content = null;

            // Act
            var response = await CallSendAsync(handler, request, CancellationToken.None);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SendAsync_ReturnsOk_WhenDisableDIIsTrue()
        {
            // Arrange
            var subscriberService = Substitute.For<ISubscriberService>();
            var gatewayService = Substitute.For<IGatewayService>();
            gatewayService.IsDisableDIAsync().Returns(true);
            
            var handler = new ValidateHandler(subscriberService, gatewayService);
            
            var request = new HttpRequestMessage();

            // Act
            var response = await CallSendAsync(handler, request, CancellationToken.None);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task SendAsync_ReturnsBadRequest_WhenSubscriberIsInvalid()
        {
            // Arrange
            var subscriberService = Substitute.For<ISubscriberService>();
            var gatewayService = Substitute.For<IGatewayService>();
            gatewayService.IsDisableDIAsync().Returns(false);
            subscriberService.ValidateAsync(Arg.Any<string>()).Returns(false);
            
            var handler = new ValidateHandler(subscriberService, gatewayService);
            
            var transactionRequest = new TransactionRequest
            {
                Subscriber = "invalid_subscriber",
                Account = "test_account"
            };
            var json = JsonConvert.SerializeObject(transactionRequest);
            var request = new HttpRequestMessage();
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await CallSendAsync(handler, request, CancellationToken.None);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            await subscriberService.Received(1).ValidateAsync("invalid_subscriber");
        }

        [Fact]
        public async Task SendAsync_ReturnsBadRequest_WhenContentIsEmpty()
        {
            // Arrange
            var subscriberService = Substitute.For<ISubscriberService>();
            var gatewayService = Substitute.For<IGatewayService>();
            gatewayService.IsDisableDIAsync().Returns(false);
            
            var handler = new ValidateHandler(subscriberService, gatewayService);
            
            var request = new HttpRequestMessage();
            request.Content = new StringContent("", Encoding.UTF8, "application/json");

            // Act
            var response = await CallSendAsync(handler, request, CancellationToken.None);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SendAsync_ReturnsBadRequest_WhenTransactionRequestIsInvalid()
        {
            // Arrange
            var subscriberService = Substitute.For<ISubscriberService>();
            var gatewayService = Substitute.For<IGatewayService>();
            gatewayService.IsDisableDIAsync().Returns(false);
            
            var handler = new ValidateHandler(subscriberService, gatewayService);
            
            var transactionRequest = new TransactionRequest
            {
                Subscriber = "", // Invalid - empty
                Account = "test_account"
            };
            var json = JsonConvert.SerializeObject(transactionRequest);
            var request = new HttpRequestMessage();
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await CallSendAsync(handler, request, CancellationToken.None);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public void ValidateHandler_ShouldBeCreated_WithValidDependencies()
        {
            // Arrange
            var subscriberService = Substitute.For<ISubscriberService>();
            var gatewayService = Substitute.For<IGatewayService>();

            // Act
            var handler = new ValidateHandler(subscriberService, gatewayService);

            // Assert
            Assert.NotNull(handler);
        }
    }
}

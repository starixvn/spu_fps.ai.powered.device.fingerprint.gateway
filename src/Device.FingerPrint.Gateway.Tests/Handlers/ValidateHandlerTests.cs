using Device.FingerPrint.Gateway.Handlers;
using Device.FingerPrint.Gateway.Models;
using Device.FingerPrint.Gateway.Services;
using Newtonsoft.Json;
using NSubstitute;
using System.Reflection;
using System.Text;

namespace Device.FingerPrint.Gateway.Tests.Handlers
{
    public class ValidateHandlerTests
    {
        private readonly ILogger mockLogger;
        private readonly ISubscriberService mockSubscriberService;
        private readonly ValidateHandler validateHandler;

        public ValidateHandlerTests()
        {
            mockLogger = Substitute.For<ILogger>();
            mockSubscriberService = Substitute.For<ISubscriberService>();

            validateHandler = new ValidateHandler(mockLogger, mockSubscriberService);
        }

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
            var request = new HttpRequestMessage();
            request.Content = null;

            // Act
            var response = await CallSendAsync(validateHandler, request, CancellationToken.None);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }


        [Fact]
        public async Task SendAsync_ReturnsBadRequest_WhenSubscriberIsInvalid()
        {
            // Arrange
            mockSubscriberService.ValidateAsync(Arg.Any<string>()).Returns(false);
            
            var transactionRequest = new TransactionRequest
            {
                Subscriber = "invalid_subscriber",
                Account = "test_account"
            };
            var json = JsonConvert.SerializeObject(transactionRequest);
            var request = new HttpRequestMessage();
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await CallSendAsync(validateHandler, request, CancellationToken.None);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            await mockSubscriberService.Received(1).ValidateAsync("invalid_subscriber");
        }

        [Fact]
        public async Task SendAsync_ReturnsBadRequest_WhenContentIsEmpty()
        {
            // Arrange
            mockSubscriberService.ValidateAsync(Arg.Any<string>()).Returns(false);

            var request = new HttpRequestMessage();
            request.Content = new StringContent("", Encoding.UTF8, "application/json");

            // Act
            var response = await CallSendAsync(validateHandler, request, CancellationToken.None);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SendAsync_ReturnsBadRequest_WhenTransactionRequestIsInvalid()
        {
            // Arrange            
            var transactionRequest = new TransactionRequest
            {
                Subscriber = "", // Invalid - empty
                Account = "test_account"
            };
            var json = JsonConvert.SerializeObject(transactionRequest);
            var request = new HttpRequestMessage();
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await CallSendAsync(validateHandler, request, CancellationToken.None);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
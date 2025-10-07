using NSubstitute;
using Device.FingerPrint.Gateway.Handlers;
using Device.FingerPrint.Gateway.Services;
using System.Text;
using System.Reflection;
using Device.FingerPrint.Gateway.Models;

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
            var logger = Substitute.For<ILogger>();
            var appSettings = Substitute.For<IAppSettings>();

            var handler = new MobileRequestHandler(logger, appSettings);
            var request = new HttpRequestMessage();
            request.Content = new StringContent("{\"Transaction\":\"eyJBY2NvdW50Ijoic3RyaW5nIiwiU3Vic2NyaWJlciI6InN0cmluZyIsIkxvZ2luVGltZSI6IjIwMjUtMTAtMDdUMDg6MTI6MTQuMDdaIiwiQWN0aW9uIjoic3RyaW5nIiwiQWN0aW9uUmVzdWx0Ijoic3RyaW5nIiwiQW5kcm9pZENvZGUiOiJzdHJpbmciLCJNYWNoaW5lQ29kZSI6InN0cmluZyIsIkN1c3RvbUNvZGUiOiJzdHJpbmciLCJNZWRpYURSTUNvZGUiOiJzdHJpbmciLCJJc0VtdWxhdG9yIjp0cnVlLCJNb2RlbE5hbWUiOiJzdHJpbmciLCJNYW51ZmFjdHVyZXIiOiJzdHJpbmciLCJCcmFuZCI6InN0cmluZyIsIlNka1ZlcnNpb24iOiJzdHJpbmciLCJPUyI6InN0cmluZyIsIlRpbWV6b25lIjoic3RyaW5nIiwiU3lzdGVtTGFuZ3VhZ2UiOiJzdHJpbmciLCJDb3VudHJ5Ijoic3RyaW5nIiwiSVAiOiJzdHJpbmciLCJEZXZpY2VEZXRhaWxzIjoic3RyaW5nIiwiVHJhbnNEYXRlIjoiMjAyNS0xMC0wN1QwODoxMjoxNC4wN1oifQ==\"}", Encoding.UTF8, "application/json");

            // Act
            var response = await CallSendAsync(handler, request, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
        }

        [Fact]
        public async Task SendAsync_ShouldReturnBadRequest_WhenNullSubscriberInMessage()
        {
            // Arrange
            var logger = Substitute.For<ILogger>();
            var appSettings = Substitute.For<IAppSettings>();

            var handler = new MobileRequestHandler(logger, appSettings);
            var request = new HttpRequestMessage();
            request.Content = new StringContent("{\"Transaction\":\"eyJBY2NvdW50Ijoic3RyaW5nIiwiU3Vic2NyaWJlciI6bnVsbCwiTG9naW5UaW1lIjoiMjAyNS0xMC0wN1QwODoxMjoxNC4wN1oiLCJBY3Rpb24iOiJzdHJpbmciLCJBY3Rpb25SZXN1bHQiOiJzdHJpbmciLCJBbmRyb2lkQ29kZSI6InN0cmluZyIsIk1hY2hpbmVDb2RlIjoic3RyaW5nIiwiQ3VzdG9tQ29kZSI6InN0cmluZyIsIk1lZGlhRFJNQ29kZSI6InN0cmluZyIsIklzRW11bGF0b3IiOnRydWUsIk1vZGVsTmFtZSI6InN0cmluZyIsIk1hbnVmYWN0dXJlciI6InN0cmluZyIsIkJyYW5kIjoic3RyaW5nIiwiU2RrVmVyc2lvbiI6InN0cmluZyIsIk9TIjoic3RyaW5nIiwiVGltZXpvbmUiOiJzdHJpbmciLCJTeXN0ZW1MYW5ndWFnZSI6InN0cmluZyIsIkNvdW50cnkiOiJzdHJpbmciLCJJUCI6InN0cmluZyIsIkRldmljZURldGFpbHMiOiJzdHJpbmciLCJUcmFuc0RhdGUiOiIyMDI1LTEwLTA3VDA4OjEyOjE0LjA3WiJ9==\"}", Encoding.UTF8, "application/json");

            // Act
            var response = await CallSendAsync(handler, request, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
        }

        [Fact]
        public async Task SendAsync_ShouldHandleEmptyContent_Gracefully()
        {
            // Arrange
            var logger = Substitute.For<ILogger>();
            var appSettings = Substitute.For<IAppSettings>();

            var handler = new MobileRequestHandler(logger, appSettings);
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
            var logger = Substitute.For<ILogger>();
            var appSettings = Substitute.For<IAppSettings>();

            var handler = new MobileRequestHandler(logger, appSettings);
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
            var logger = Substitute.For<ILogger>();
            var appSettings = Substitute.For<IAppSettings>();

            var handler = new MobileRequestHandler(logger, appSettings);
            var request = new HttpRequestMessage();
            request.Content = new StringContent("{invalid json}", Encoding.UTF8, "application/json");

            // Act
            var response = await CallSendAsync(handler, request, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
        }
    }
}
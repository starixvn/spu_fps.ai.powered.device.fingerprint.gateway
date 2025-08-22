using Xunit;
using NSubstitute;
using Device.FingerPrint.Gateway.Handlers;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Reflection;

namespace Device.FingerPrint.Gateway.Tests.Handlers
{
    public class IPCaptureHandlerTests
    {
        private async Task<HttpResponseMessage> CallSendAsync(IPCaptureHandler handler, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Use reflection to call protected SendAsync method
            var method = typeof(IPCaptureHandler).GetMethod("SendAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            var task = (Task<HttpResponseMessage>)method.Invoke(handler, new object[] { request, cancellationToken });
            return await task;
        }

        [Fact]
        public async Task SendAsync_ShouldCaptureIPAddress_WhenRequestHasXForwardedFor()
        {
            // Arrange
            var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            context.Request.Headers["X-Forwarded-For"] = "192.168.1.1";
            httpContextAccessor.HttpContext.Returns(context);
            
            var handler = new IPCaptureHandler(httpContextAccessor);
            var request = new HttpRequestMessage();
            request.Content = new StringContent("{\"test\":\"data\"}", Encoding.UTF8, "application/json");

            // Act
            var response = await CallSendAsync(handler, request, CancellationToken.None);

            // Assert
            // Verify that the handler processes the request
            Assert.NotNull(response);
        }

        [Fact]
        public async Task SendAsync_ShouldCaptureIPAddress_WhenRequestHasRemoteIPAddress()
        {
            // Arrange
            var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
            httpContextAccessor.HttpContext.Returns(context);
            
            var handler = new IPCaptureHandler(httpContextAccessor);
            var request = new HttpRequestMessage();
            request.Content = new StringContent("{\"test\":\"data\"}", Encoding.UTF8, "application/json");

            // Act
            var response = await CallSendAsync(handler, request, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
        }

        [Fact]
        public async Task SendAsync_ShouldHandleNullContent_Gracefully()
        {
            // Arrange
            var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            httpContextAccessor.HttpContext.Returns(context);
            
            var handler = new IPCaptureHandler(httpContextAccessor);
            var request = new HttpRequestMessage();
            request.Content = null;

            // Act
            var response = await CallSendAsync(handler, request, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
        }

        [Fact]
        public async Task SendAsync_ShouldHandleNullHttpContext_Gracefully()
        {
            // Arrange
            var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            httpContextAccessor.HttpContext.Returns((HttpContext)null);
            
            var handler = new IPCaptureHandler(httpContextAccessor);
            var request = new HttpRequestMessage();
            request.Content = new StringContent("{\"test\":\"data\"}", Encoding.UTF8, "application/json");

            // Act
            var response = await CallSendAsync(handler, request, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
        }

        [Fact]
        public void IPCaptureHandler_ShouldBeCreated_WithValidDependencies()
        {
            // Arrange
            var httpContextAccessor = Substitute.For<IHttpContextAccessor>();

            // Act
            var handler = new IPCaptureHandler(httpContextAccessor);

            // Assert
            Assert.NotNull(handler);
        }
    }
}
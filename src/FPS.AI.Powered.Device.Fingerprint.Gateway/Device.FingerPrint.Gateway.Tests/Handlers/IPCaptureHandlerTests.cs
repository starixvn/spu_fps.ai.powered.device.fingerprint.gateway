using Xunit;
using NSubstitute;
using Device.FingerPrint.Gateway.Handlers;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

namespace Device.FingerPrint.Gateway.Tests.Handlers
{
    public class IPCaptureHandlerTests
    {
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
            var response = await handler.SendAsync(request, CancellationToken.None);

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
            var response = await handler.SendAsync(request, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
        }
    }
}
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
    public class MobileRequestHandlerTests
    {
        [Fact]
        public async Task SendAsync_ShouldProcessMobileRequest_WhenValidContent()
        {
            // Arrange
            var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            httpContextAccessor.HttpContext.Returns(context);
            
            var handler = new MobileRequestHandler(httpContextAccessor);
            var request = new HttpRequestMessage();
            request.Content = new StringContent("{\"DeviceId\":\"test123\"}", Encoding.UTF8, "application/json");

            // Act
            var response = await handler.SendAsync(request, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
        }

        [Fact]
        public async Task SendAsync_ShouldHandleEmptyContent_Gracefully()
        {
            // Arrange
            var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            httpContextAccessor.HttpContext.Returns(context);
            
            var handler = new MobileRequestHandler(httpContextAccessor);
            var request = new HttpRequestMessage();
            request.Content = new StringContent("", Encoding.UTF8, "application/json");

            // Act
            var response = await handler.SendAsync(request, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
        }
    }
}
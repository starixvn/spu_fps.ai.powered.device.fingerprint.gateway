using Xunit;
using NSubstitute;
using Device.FingerPrint.Gateway.Controllers;
using Device.FingerPrint.Gateway.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Device.FingerPrint.Gateway.Tests.Controllers
{
    public class CommonControllerTests
    {
        [Fact]
        public void Index_Returns_WhoamiModel()
        {
            // Arrange
            var appSettings = Substitute.For<IAppSettings>();
            appSettings.Environment.Returns("TEST");
            
            var controller = new CommonController(appSettings);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            
            // Mock the IHttpConnectionFeature
            var connectionFeature = Substitute.For<IHttpConnectionFeature>();
            connectionFeature.LocalIpAddress.Returns(System.Net.IPAddress.Loopback);
            controller.ControllerContext.HttpContext.Features.Set(connectionFeature);

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsType<WhoamiModel>(result);
            Assert.Equal("TEST", result.Environment);
        }

        [Fact]
        public void Index_Sets_ServerName_And_ServerTime()
        {
            // Arrange
            var appSettings = Substitute.For<IAppSettings>();
            appSettings.Environment.Returns("TEST");
            
            var controller = new CommonController(appSettings);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            
            // Mock the IHttpConnectionFeature
            var connectionFeature = Substitute.For<IHttpConnectionFeature>();
            connectionFeature.LocalIpAddress.Returns(System.Net.IPAddress.Loopback);
            controller.ControllerContext.HttpContext.Features.Set(connectionFeature);

            // Act
            var result = controller.Index();

            // Assert
            Assert.NotNull(result.ServerName);
            Assert.True(result.ServerTime > DateTime.MinValue);
        }

        [Fact]
        public void CommonController_ShouldBeCreated_WithValidDependencies()
        {
            // Arrange
            var appSettings = Substitute.For<IAppSettings>();

            // Act
            var controller = new CommonController(appSettings);

            // Assert
            Assert.NotNull(controller);
        }
    }
}
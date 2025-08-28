using NSubstitute;
using Device.FingerPrint.Gateway.Controllers;
using Device.FingerPrint.Gateway.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Device.FingerPrint.Gateway.Tests.Controllers
{
    public class CommonControllerTests
    {
        private readonly IAppSettings mockAppSettings;
        private readonly IHttpConnectionFeature mockHttpConnectionFeature;
        private CommonController controller;

        public CommonControllerTests()
        {
            mockAppSettings = Substitute.For<IAppSettings>();
            mockAppSettings.Environment.Returns("TEST");

            mockHttpConnectionFeature = Substitute.For<IHttpConnectionFeature>();
            mockHttpConnectionFeature.LocalIpAddress.Returns(System.Net.IPAddress.Loopback);

            controller = new CommonController(mockAppSettings);
        }

        [Fact]
        public void Index_Returns_WhoamiModel()
        {
            // Arrange
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set(mockHttpConnectionFeature);

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
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Features.Set(mockHttpConnectionFeature);

            // Act
            var result = controller.Index();

            // Assert
            Assert.NotNull(result.ServerName);
            Assert.True(result.ServerTime > DateTime.MinValue);
        }
    }
}
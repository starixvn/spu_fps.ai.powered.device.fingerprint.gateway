using Xunit;
using Device.FingerPrint.Gateway.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Device.FingerPrint.Gateway.Tests.Controllers
{
    public class CommonControllerTests
    {
        [Fact]
        public void Get_Returns_OkResult()
        {
            // Arrange
            var controller = new CommonController();

            // Act
            var result = controller.Get();

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public void Get_Returns_StatusCode200()
        {
            // Arrange
            var controller = new CommonController();

            // Act
            var result = controller.Get() as OkResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }
    }
}

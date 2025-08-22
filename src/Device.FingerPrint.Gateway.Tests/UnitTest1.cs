using Xunit;
using NSubstitute;
using Device.FingerPrint.Gateway.Handlers;
using Device.FingerPrint.Gateway.Services;
using Device.FingerPrint.Gateway.Model;
using Newtonsoft.Json;
using System.Text;

namespace Device.FingerPrint.Gateway.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void SimpleTest_ShouldPass()
        {
            // Arrange
            var expected = "Hello World";
            
            // Act
            var actual = "Hello World";
            
            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ValidateHandler_ShouldBeCreated_WithDependencies()
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

using Device.FingerPrint.Gateway.Models;

namespace Device.FingerPrint.Gateway.Tests.Models
{
    public class SubscriberModelTests
    {
        [Fact]
        public void SubscriberModel_ShouldSetProperties_Correctly()
        {
            // Arrange
            var model = new SubscriberModel();

            // Act
            model.SubscriberId = 123;
            model.SubscriberName = "TestSubscriber";

            // Assert
            Assert.Equal(123, model.SubscriberId);
            Assert.Equal("TestSubscriber", model.SubscriberName);
        }

        [Fact]
        public void SubscriberModel_ShouldInitialize_WithDefaultValues()
        {
            // Act
            var model = new SubscriberModel();

            // Assert
            Assert.Equal(0, model.SubscriberId);
            Assert.Null(model.SubscriberName);
        }
    }

    public class MobileTransactionModelTests
    {
        [Fact]
        public void MobileTransactionModel_ShouldSetProperties_Correctly()
        {
            // Arrange
            var model = new MobileTransactionModel();
            var testDate = DateTime.Now;

            // Act
            model.Account = "TestAccount";
            model.Subscriber = "TestSubscriber";
            model.LoginTime = testDate;
            model.Action = "Login";
            model.ActionResult = "Success";
            model.AndroidCode = "ANDROID123";
            model.MachineCode = "MACHINE123";
            model.IsEmulator = false;
            model.ModelName = "TestModel";
            model.Manufacturer = "TestManufacturer";
            model.Brand = "TestBrand";
            model.SdkVersion = "API30";
            model.OS = "Android 11";
            model.Timezone = "UTC+7";
            model.SystemLanguage = "en-US";
            model.Country = "VN";
            model.DeviceDetails = "DeviceDetails";
            model.TransDate = testDate;

            // Assert
            Assert.Equal("TestAccount", model.Account);
            Assert.Equal("TestSubscriber", model.Subscriber);
            Assert.Equal(testDate, model.LoginTime);
            Assert.Equal("Login", model.Action);
            Assert.Equal("Success", model.ActionResult);
            Assert.Equal("ANDROID123", model.AndroidCode);
            Assert.Equal("MACHINE123", model.MachineCode);
            Assert.False(model.IsEmulator);
            Assert.Equal("TestModel", model.ModelName);
            Assert.Equal("TestManufacturer", model.Manufacturer);
            Assert.Equal("TestBrand", model.Brand);
            Assert.Equal("API30", model.SdkVersion);
            Assert.Equal("Android 11", model.OS);
            Assert.Equal("UTC+7", model.Timezone);
            Assert.Equal("en-US", model.SystemLanguage);
            Assert.Equal("VN", model.Country);
            Assert.Equal("DeviceDetails", model.DeviceDetails);
            Assert.Equal(testDate, model.TransDate);
        }
    }

    public class MobileTransactionRequestTests
    {
        [Fact]
        public void MobileTransactionRequest_ShouldSetTransaction_Correctly()
        {
            // Arrange
            var model = new MobileTransactionRequest();
            var transactionJson = "{\"Account\":\"TestAccount\",\"DeviceId\":\"test123\"}";

            // Act
            model.Transaction = transactionJson;

            // Assert
            Assert.NotNull(model.Transaction);
            Assert.Equal(transactionJson, model.Transaction);
        }

        [Fact]
        public void MobileTransactionRequest_ShouldInitialize_WithNullTransaction()
        {
            // Act
            var model = new MobileTransactionRequest();

            // Assert
            Assert.Null(model.Transaction);
        }
    }
}
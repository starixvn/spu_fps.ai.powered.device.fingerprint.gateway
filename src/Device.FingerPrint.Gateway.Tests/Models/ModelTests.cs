namespace Device.FingerPrint.Gateway.Tests.Models
{
    public class TransactionResultTests
    {
        [Fact]
        public void GetMessage_ReturnsCorrectMessage_ForValidCode()
        {
            // Act
            var message = TransactionResult.GetMessage("000");

            // Assert
            Assert.Equal("Success", message);
        }

        [Fact]
        public void GetMessage_ReturnsUnknownMessage_ForInvalidCode()
        {
            // Act
            var message = TransactionResult.GetMessage("99");

            // Assert
            Assert.Equal("Unknown error code", message);
        }

        [Fact]
        public void GetMessage_ReturnsUnknownMessage_ForNullCode()
        {
            // Act
            var message = TransactionResult.GetMessage(null);

            // Assert
            Assert.Equal("Unknown error code", message);
        }

        [Fact]
        public void GetMessage_ReturnsUnknownMessage_ForEmptyCode()
        {
            // Act
            var message = TransactionResult.GetMessage("");

            // Assert
            Assert.Equal("Unknown error code", message);
        }

        [Fact]
        public void GetMessage_ReturnsCorrectMessage_ForAllDefinedCodes()
        {
            // Act & Assert
            Assert.Equal("Success", TransactionResult.GetMessage("000"));
            Assert.Equal("Allow", TransactionResult.GetMessage("001"));
            Assert.Equal("Review", TransactionResult.GetMessage("002"));
            Assert.Equal("Deny", TransactionResult.GetMessage("003"));
            Assert.Equal("Account is empty", TransactionResult.GetMessage("006"));
            Assert.Equal("Device is empty", TransactionResult.GetMessage("007"));
            Assert.Equal("Action doesn't exist", TransactionResult.GetMessage("008"));
            Assert.Equal("Subscriber is invalid", TransactionResult.GetMessage("009"));
            Assert.Equal("Under Maintenance", TransactionResult.GetMessage("010"));
            Assert.Equal("Other error", TransactionResult.GetMessage("1000"));
        }
    }

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
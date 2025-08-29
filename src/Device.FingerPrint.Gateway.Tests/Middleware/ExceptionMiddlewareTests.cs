using NSubstitute;
using Microsoft.AspNetCore.Http;
using Device.FingerPrint.Gateway.Middlewares;

namespace Device.FingerPrint.Gateway.Tests.Middleware
{
    public class ExceptionMiddlewareTests
    {
        private readonly ILogger logger;

        public ExceptionMiddlewareTests()
        {
            logger = Substitute.For<ILogger>();
        }

        [Fact]
        public async Task InvokeAsync_ShouldCallNext_WhenNoExceptionThrown()
        {
            // Arrange
            var next = Substitute.For<RequestDelegate>();
            var context = new DefaultHttpContext();
            
            var middleware = new ExceptionMiddleware(next, logger);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            await next.Received(1).Invoke(context);
        }

        [Fact]
        public async Task InvokeAsync_ShouldLogException_WhenNextThrowsException()
        {
            // Arrange
            var next = Substitute.For<RequestDelegate>();
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            
            next.When(x => x.Invoke(Arg.Any<HttpContext>()))
                .Do(x => throw new Exception("Test exception"));
            
            var middleware = new ExceptionMiddleware(next, logger);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            await logger.Received(1).ErrorAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task InvokeAsync_ShouldLogIOException_WhenIOExceptionOccurs()
        {
            // Arrange
            var next = Substitute.For<RequestDelegate>();
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            
            var testException = new IOException("IO Test exception");
            next.When(x => x.Invoke(Arg.Any<HttpContext>()))
                .Do(x => throw testException);
            
            var middleware = new ExceptionMiddleware(next, logger);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            await logger.Received(1).ErrorAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task InvokeAsync_ShouldContinue_WhenExceptionOccurs()
        {
            // Arrange
            var next = Substitute.For<RequestDelegate>();
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            
            next.When(x => x.Invoke(Arg.Any<HttpContext>()))
                .Do(x => throw new Exception("Test exception"));
            
            var middleware = new ExceptionMiddleware(next, logger);

            // Act & Assert - Should not throw exception
            await middleware.InvokeAsync(context);
            
            // Verify that the exception was caught and logged
            await logger.Received(1).ErrorAsync(Arg.Any<string>());
        }

        [Fact]
        public void ExceptionMiddleware_ShouldBeCreated_WithValidDependencies()
        {
            // Arrange
            var next = Substitute.For<RequestDelegate>();

            // Act
            var middleware = new ExceptionMiddleware(next, logger);

            // Assert
            Assert.NotNull(middleware);
        }
    }
}
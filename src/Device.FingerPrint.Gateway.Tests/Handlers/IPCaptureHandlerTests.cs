namespace Device.FingerPrint.Gateway.Tests.Handlers;

using Device.FingerPrint.Gateway.Handlers;
using Fanex.Logging;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Net;
using System.Reflection;
using System.Text;

public class IPCaptureHandlerTests
{
    private readonly ILogger mockLogger;
    private readonly IHttpContextAccessor mockHttpContextAccessor;
    private readonly IPCaptureHandler _ipCaptureHandler;

    public IPCaptureHandlerTests()
    {
        mockLogger = Substitute.For<ILogger>();
        mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _ipCaptureHandler = new IPCaptureHandler(mockLogger, mockHttpContextAccessor);
    }

    private async Task<HttpResponseMessage> CallSendAsync(IPCaptureHandler handler, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Use reflection to call protected SendAsync method
        var method = typeof(IPCaptureHandler).GetMethod("SendAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        var task = (Task<HttpResponseMessage>)method.Invoke(handler, new object[] { request, cancellationToken });
        return await task;
    }

    // Custom mock handler to replace the inner handler
    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public MockHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_response);
        }
    }

    [Fact]
    public async Task SendAsync_ShouldAddFpsForwardedForHeader_WhenXForwardedForExists()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Forwarded-For"] = "192.168.1.100";
        mockHttpContextAccessor.HttpContext.Returns(context);

        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var mockInnerHandler = new MockHttpMessageHandler(expectedResponse);
        _ipCaptureHandler.InnerHandler = mockInnerHandler;

        var request = new HttpRequestMessage(HttpMethod.Post, "http://test.com");
        request.Content = new StringContent("{\"test\":\"data\"}", Encoding.UTF8, "application/json");

        // Act
        var response = await CallSendAsync(_ipCaptureHandler, request, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(request.Headers.Contains("Fps-Forwarded-For"));
        Assert.Equal("192.168.1.100", request.Headers.GetValues("Fps-Forwarded-For").FirstOrDefault());
    }

    [Fact]
    public async Task SendAsync_ShouldAddFpsForwardedForHeader_WhenRemoteIpAddressExists()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
        mockHttpContextAccessor.HttpContext.Returns(context);

        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var mockInnerHandler = new MockHttpMessageHandler(expectedResponse);
        _ipCaptureHandler.InnerHandler = mockInnerHandler;

        var request = new HttpRequestMessage(HttpMethod.Post, "http://test.com");
        request.Content = new StringContent("{\"test\":\"data\"}", Encoding.UTF8, "application/json");

        // Act
        var response = await CallSendAsync(_ipCaptureHandler, request, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(request.Headers.Contains("Fps-Forwarded-For"));
        Assert.Equal("127.0.0.1", request.Headers.GetValues("Fps-Forwarded-For").FirstOrDefault());
    }

    [Fact]
    public async Task SendAsync_ShouldPreferXForwardedFor_OverRemoteIpAddress()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Forwarded-For"] = "10.0.0.1";
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");
        mockHttpContextAccessor.HttpContext.Returns(context);

        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var mockInnerHandler = new MockHttpMessageHandler(expectedResponse);
        _ipCaptureHandler.InnerHandler = mockInnerHandler;

        var request = new HttpRequestMessage(HttpMethod.Post, "http://test.com");
        request.Content = new StringContent("{\"test\":\"data\"}", Encoding.UTF8, "application/json");

        // Act
        var response = await CallSendAsync(_ipCaptureHandler, request, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(request.Headers.Contains("Fps-Forwarded-For"));
        Assert.Equal("10.0.0.1", request.Headers.GetValues("Fps-Forwarded-For").FirstOrDefault());
    }

    [Fact]
    public async Task SendAsync_ShouldNotAddHeader_WhenNoIpAddressAvailable()
    {
        // Arrange
        var context = new DefaultHttpContext();
        // No X-Forwarded-For header and no RemoteIpAddress
        mockHttpContextAccessor.HttpContext.Returns(context);

        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var mockInnerHandler = new MockHttpMessageHandler(expectedResponse);
        _ipCaptureHandler.InnerHandler = mockInnerHandler;

        var request = new HttpRequestMessage(HttpMethod.Post, "http://test.com");
        request.Content = new StringContent("{\"test\":\"data\"}", Encoding.UTF8, "application/json");

        // Act
        var response = await CallSendAsync(_ipCaptureHandler, request, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(request.Headers.Contains("Fps-Forwarded-For"));
    }

    [Fact]
    public async Task SendAsync_ShouldNotAddHeader_WhenXForwardedForIsEmpty()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Forwarded-For"] = "";
        mockHttpContextAccessor.HttpContext.Returns(context);

        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var mockInnerHandler = new MockHttpMessageHandler(expectedResponse);
        _ipCaptureHandler.InnerHandler = mockInnerHandler;

        var request = new HttpRequestMessage(HttpMethod.Post, "http://test.com");
        request.Content = new StringContent("{\"test\":\"data\"}", Encoding.UTF8, "application/json");

        // Act
        var response = await CallSendAsync(_ipCaptureHandler, request, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(request.Headers.Contains("Fps-Forwarded-For"));
    }

    [Fact]
    public async Task SendAsync_ShouldReturnBadRequest_WhenRequestIsNull()
    {
        // Arrange
        var context = new DefaultHttpContext();
        mockHttpContextAccessor.HttpContext.Returns(context);

        // Act
        var response = await CallSendAsync(_ipCaptureHandler, null, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SendAsync_ShouldReturnBadRequest_WhenHttpContextIsNull()
    {
        // Arrange
        mockHttpContextAccessor.HttpContext.Returns((HttpContext)null);

        var request = new HttpRequestMessage(HttpMethod.Post, "http://test.com");
        request.Content = new StringContent("{\"test\":\"data\"}", Encoding.UTF8, "application/json");

        // Act
        var response = await CallSendAsync(_ipCaptureHandler, request, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SendAsync_ShouldHandleMultipleXForwardedForValues()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Forwarded-For"] = "192.168.1.1, 10.0.0.1, 172.16.0.1";
        mockHttpContextAccessor.HttpContext.Returns(context);

        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var mockInnerHandler = new MockHttpMessageHandler(expectedResponse);
        _ipCaptureHandler.InnerHandler = mockInnerHandler;

        var request = new HttpRequestMessage(HttpMethod.Post, "http://test.com");
        request.Content = new StringContent("{\"test\":\"data\"}", Encoding.UTF8, "application/json");

        // Act
        var response = await CallSendAsync(_ipCaptureHandler, request, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(request.Headers.Contains("Fps-Forwarded-For"));
        Assert.Equal("192.168.1.1, 10.0.0.1, 172.16.0.1", request.Headers.GetValues("Fps-Forwarded-For").FirstOrDefault());
    }

    [Fact]
    public void IPCaptureHandler_ShouldBeCreated_WithValidDependencies()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();

        // Act
        var handler = new IPCaptureHandler(logger, httpContextAccessor);

        // Assert
        Assert.NotNull(handler);
    }
}
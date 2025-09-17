namespace Device.FingerPrint.Gateway.Handlers
{
    using Fanex.Logging;

    public class IPCaptureHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public IPCaptureHandler(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                if (request != null )
                {
                    var ipAddress = httpContextAccessor.HttpContext?.Request?.Headers["X-Forwarded-For"].ToString();

                    if (string.IsNullOrEmpty(ipAddress) && httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress != null)
                        ipAddress = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

                    if(!string.IsNullOrEmpty(ipAddress))
                        request.Headers.Add("Fps-Forwarded-For", ipAddress);

                    return await base.SendAsync(request, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                LogService.Error(ex.Message, ex);
            }

            return new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest
            };
        }
    }
}

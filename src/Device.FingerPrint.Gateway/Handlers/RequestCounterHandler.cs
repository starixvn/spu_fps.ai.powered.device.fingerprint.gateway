using Device.FingerPrint.Gateway.Services;

namespace Device.FingerPrint.Gateway.Handlers
{
    public class RequestCounterHandler : DelegatingHandler
    {
        private readonly RequestCounterService _requestCounterService;

        public RequestCounterHandler()
        {
            _requestCounterService = RequestCounterService.GetInstance();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _requestCounterService.Increment();
            return await base.SendAsync(request, cancellationToken);
        }
    }
}

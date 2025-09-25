using Device.FingerPrint.Gateway.Models;
using Device.FingerPrint.Gateway.Services;
using Newtonsoft.Json;
using System.Text;

namespace Device.FingerPrint.Gateway.Handlers
{
    public class ValidateHandler : DelegatingHandler
    {
        private readonly ILogger _logger;
        private readonly ISubscriberService subscriberService;
        private readonly IGatewayService gatewayService;

        private static readonly HashSet<string> DebugAccounts = new HashSet<string> { "qctest5oct202019", "qatest05", "testJo0101", "andy7" };

        public ValidateHandler(ILogger logger, ISubscriberService subscriberService, IGatewayService gatewayService)
        {
            _logger = logger;
            this.subscriberService = subscriberService;
            this.gatewayService = gatewayService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                if (await IsGatewayDisabledAsync())
                {
                    return CreateOkResponse();
                }

                if (!IsValidRequest(request))
                {
                    return CreateBadRequestResponse();
                }

                var transactionRequest = await ParseAndValidateTransactionAsync(request);
                if (transactionRequest == null)
                {
                    return CreateBadRequestResponse();
                }

                await LogDebugAccountIfNeededAsync(transactionRequest);

                if (!await IsValidSubscriberAsync(transactionRequest.Subscriber))
                {
                    await _logger.InfoAsync($"Gateway Subscriber invalid:\n {JsonConvert.SerializeObject(transactionRequest)}");
                    return CreateBadRequestResponse();
                }

                return await ForwardValidTransactionAsync(transactionRequest, request, cancellationToken);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
                return CreateBadRequestResponse();
            }
        }

        private async Task<bool> IsGatewayDisabledAsync()
        {
            return await gatewayService.IsDisableDIAsync();
        }

        private static bool IsValidRequest(HttpRequestMessage request)
        {
            return request?.Content != null;
        }

        private async Task<TransactionRequest?> ParseAndValidateTransactionAsync(HttpRequestMessage request)
        {
            var content = await request.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
            {
                await _logger.InfoAsync($"Gateway cannot read request.Content:\n {JsonConvert.SerializeObject(request.Content)}");
                return null;
            }

            var transactionRequest = JsonConvert.DeserializeObject<TransactionRequest>(content);

            if (!IsValidTransactionRequest(transactionRequest))
            {
                await _logger.InfoAsync($"Gateway TransactionRequest invalid:\n {JsonConvert.SerializeObject(transactionRequest)}");
                return null;
            }

            return transactionRequest;
        }

        private static bool IsValidTransactionRequest(TransactionRequest? transactionRequest)
        {
            return transactionRequest != null && 
                   !string.IsNullOrWhiteSpace(transactionRequest.Subscriber) && 
                   !string.IsNullOrWhiteSpace(transactionRequest.Account);
        }

        private async Task LogDebugAccountIfNeededAsync(TransactionRequest transactionRequest)
        {
            if (DebugAccounts.Contains(transactionRequest.Account.ToLower(System.Globalization.CultureInfo.CurrentCulture)))
            {
                await _logger.InfoAsync($"Gateway Debug for Account:{transactionRequest.Account}\n {JsonConvert.SerializeObject(transactionRequest)}");
            }
        }

        private async Task<bool> IsValidSubscriberAsync(string subscriber)
        {
            return await subscriberService.ValidateAsync(subscriber);
        }

        private async Task<HttpResponseMessage> ForwardValidTransactionAsync(TransactionRequest transactionRequest, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            transactionRequest.TransDate = DateTimeOffset.Now.DateTime;
            request.Content = new StringContent(JsonConvert.SerializeObject(transactionRequest), Encoding.UTF8, "application/json");
            return await base.SendAsync(request, cancellationToken);
        }

        private static HttpResponseMessage CreateOkResponse()
        {
            return new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK
            };
        }

        private static HttpResponseMessage CreateBadRequestResponse()
        {
            return new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest
            };
        }
    }
}
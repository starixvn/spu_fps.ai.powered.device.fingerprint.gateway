namespace Device.FingerPrint.Gateway.Handlers
{
    using Device.FingerPrint.Gateway.Model;
    using Device.FingerPrint.Gateway.Services;
    using Fanex.Logging;
    using Newtonsoft.Json;
    using System.Text;

    public class ValidateHandler : DelegatingHandler
    {
        private readonly ISubscriberService subscriberService;
        private readonly IGatewayService gatewayService;

        private static readonly HashSet<string> DebugAccounts = new HashSet<string> { "qctest5oct202019", "qatest05" };

        public ValidateHandler(ISubscriberService subscriberService, IGatewayService gatewayService)
        {
            this.subscriberService = subscriberService;
            this.gatewayService = gatewayService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                var isDisableDI = await gatewayService.IsDisableDIAsync();
                if (isDisableDI)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }

                if (request != null && request.Content != null && !isDisableDI)
                {
                    var content = await request.Content.ReadAsStringAsync();

                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        var transactionRequest = JsonConvert.DeserializeObject<TransactionRequest>(content);

                        if (transactionRequest != null && !string.IsNullOrWhiteSpace(transactionRequest?.Subscriber) && !string.IsNullOrWhiteSpace(transactionRequest.Account))
                        {
                            if (DebugAccounts.Contains(transactionRequest.Account.ToLower()))
                            {
                                _ = LogService.InfoAsync($"Gateway Debug for Account:{transactionRequest.Account}\n {JsonConvert.SerializeObject(transactionRequest)}");
                            }

                            if (await this.subscriberService.ValidateAsync(transactionRequest?.Subscriber))
                            {
                                transactionRequest.TransDate = DateTime.Now;
                                request.Content = new StringContent(JsonConvert.SerializeObject(transactionRequest), Encoding.UTF8, "application/json");

                                return await base.SendAsync(request, cancellationToken);
                            }
                            else
                            {
                                _ = LogService.InfoAsync($"Gateway Subscriber invalid:\n {JsonConvert.SerializeObject(transactionRequest)}");
                            }
                        }
                        else
                        {
                            _ = LogService.InfoAsync($"Gateway TransactionRequest invalid:\n {JsonConvert.SerializeObject(transactionRequest)}");
                        }
                    }
                    else
                    {
                        _ = LogService.InfoAsync($"Gateway cannot read request.Content:\n {JsonConvert.SerializeObject(request.Content)}");
                    }
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

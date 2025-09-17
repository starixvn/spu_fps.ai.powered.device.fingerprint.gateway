namespace Device.FingerPrint.Gateway.Handlers
{
    using Device.FingerPrint.Gateway.Model;
    using Device.FingerPrint.Gateway.Services;
    using Fanex.Logging;
    using Newtonsoft.Json;
    using System.Text;

    public class MobileRequestHandler : DelegatingHandler
    {
        private readonly ISubscriberService subscriberService;
        private readonly IGatewayService gatewayService;

        private static readonly HashSet<string> DebugAccounts = new HashSet<string> { "member19", "2hcibc0926a" };

        public MobileRequestHandler(ISubscriberService subscriberService, IGatewayService gatewayService)
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
                        var transactionRequest = JsonConvert.DeserializeObject<MobileTransactionRequest>(content);

                        if (transactionRequest != null)
                        {
                            byte[] base64DecodedBytes = Convert.FromBase64String(transactionRequest.Transaction);

                            string decodedString = System.Text.Encoding.UTF8.GetString(base64DecodedBytes);

                            var transaction = JsonConvert.DeserializeObject<MobileTransactionModel>(decodedString);

                            if (transactionRequest != null && !string.IsNullOrWhiteSpace(transaction?.Subscriber) && !string.IsNullOrWhiteSpace(transaction.Account))
                            {
                                if (DebugAccounts.Contains(transaction.Account.ToLower().Trim()))
                                {
                                    _ = LogService.InfoAsync($"Gateway Debug for Account:{transaction.Account}\n {JsonConvert.SerializeObject(transaction)}");
                                }

                                if (!string.IsNullOrEmpty(transaction.Exceptions))
                                {
                                    _ = LogService.InfoAsync($"Mobile App Request Occurs Exception: {transaction.Exceptions}\n {JsonConvert.SerializeObject(transaction)}");
                                }

                                if (await this.subscriberService.ValidateAsync(transaction.Subscriber))
                                {
                                    transaction.TransDate = DateTime.Now;
                                    request.Content = new StringContent(JsonConvert.SerializeObject(transaction), Encoding.UTF8, "application/json");

                                    return await base.SendAsync(request, cancellationToken);
                                    
                                }
                            }
                            else
                            {
                                return new HttpResponseMessage
                                {
                                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                                    Content = new StringContent("Subscriber or Account cannot be null.")
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogService.Error($"{ex.Message} \n {request.Content}", ex);
            }

            return new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest
            };
        }
    }
}

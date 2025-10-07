using Device.FingerPrint.Gateway.Models;
using Newtonsoft.Json;
using System.Text;

namespace Device.FingerPrint.Gateway.Handlers
{
    public class MobileRequestHandler : DelegatingHandler
    {
        private readonly ILogger logger;
        private readonly IAppSettings appSettings;

        private static readonly HashSet<string> DebugAccounts = new HashSet<string> { "member19", "2hcibc0926a" };

        public MobileRequestHandler(ILogger logger, IAppSettings appSettings)
        {
            this.logger = logger;
            this.appSettings = appSettings;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                if (!IsValidRequest(request))
                {
                    return CreateBadRequestResponse();
                }

                var mobileTransaction = await ParseAndValidateMobileTransactionAsync(request);
                if (mobileTransaction == null)
                {
                    return CreateBadRequestResponse();
                }

                await ProcessDebugAndExceptionLoggingAsync(mobileTransaction);

                return await ForwardValidTransactionAsync(mobileTransaction, request, cancellationToken);
            }
            catch (Exception ex)
            {
                await logger.ErrorAsync($"{ex.Message} \n {request.Content}", ex);
                return CreateBadRequestResponse();
            }
        }

        private static bool IsValidRequest(HttpRequestMessage request)
        {
            return request?.Content != null;
        }

        private async Task<MobileTransactionModel?> ParseAndValidateMobileTransactionAsync(HttpRequestMessage request)
        {
            var content = await request.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            var transactionRequest = JsonConvert.DeserializeObject<MobileTransactionRequest>(content);
            if (transactionRequest?.Transaction == null)
            {
                return null;
            }

            var decodedTransaction = DecodeBase64Transaction(transactionRequest.Transaction);
            if (decodedTransaction == null)
            {
                return null;
            }

            if (!IsValidMobileTransaction(decodedTransaction))
            {
                return null;
            }

            return decodedTransaction;
        }

        private static MobileTransactionModel? DecodeBase64Transaction(string base64Transaction)
        {
            try
            {
                byte[] base64DecodedBytes = Convert.FromBase64String(base64Transaction);
                string decodedString = System.Text.Encoding.UTF8.GetString(base64DecodedBytes);
                return JsonConvert.DeserializeObject<MobileTransactionModel>(decodedString);
            }
            catch
            {
                return null;
            }
        }

        private static bool IsValidMobileTransaction(MobileTransactionModel? transaction)
        {
            return transaction != null && 
                   !string.IsNullOrWhiteSpace(transaction.Subscriber) && 
                   !string.IsNullOrWhiteSpace(transaction.Account);
        }

        private async Task ProcessDebugAndExceptionLoggingAsync(MobileTransactionModel transaction)
        {
            if(!appSettings.EnableMBDebugLogging)
            {
                return;
            }

            if (DebugAccounts.Contains(transaction.Account.ToLower(System.Globalization.CultureInfo.CurrentCulture).Trim()))
            {
                await logger.InfoAsync($"Gateway Debug for Account:{transaction.Account}\n {JsonConvert.SerializeObject(transaction)}");
            }

            if (!string.IsNullOrEmpty(transaction.Exceptions))
            {
                await logger.InfoAsync($"Mobile App Request Occurs Exception\n {transaction.Exceptions}\n {JsonConvert.SerializeObject(transaction)}");
            }
        }

        private async Task<HttpResponseMessage> ForwardValidTransactionAsync(MobileTransactionModel transaction, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Content = new StringContent(JsonConvert.SerializeObject(transaction), Encoding.UTF8, "application/json");
            return await base.SendAsync(request, cancellationToken);
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

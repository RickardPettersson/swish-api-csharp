using Newtonsoft.Json;
using SwishApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SwishApi
{
    public class RefundClient
    {
        readonly string _environment;
        readonly string _callbackUrl;
        readonly string _payeePaymentReference;
        readonly ClientCertificate _certificate;
        readonly bool _enableHTTPLog;

        /// <summary>
        /// Construct a Refund client for Swish Payment, with certificate installed on server
        /// </summary>
        /// <param name="apiBaseUrl">Base URL for the Swish Payment API</param>
        /// <param name="callbackUrl">URL where you like to get the Swish Payment Callback</param>
        /// <param name="payeePaymentReference">Payment reference supplied by theMerchant. This is not used by Swish but is included in responses back to the client. This reference could for example be an order id or similar. If set the value must not exceed 35 characters and only the following characters are allowed: [a-ö, A-Ö, 0-9, -]</param>
        /// <param name="enableHTTPLog">Set to true to log HTTP Requests to the Swish Payment API</param>
        /// /// <param name="environment">Set what environment of Swish Payment API should be used, PROD, SANDBOX or EMULATOR</param>
        public RefundClient(string callbackUrl, string payeePaymentReference, bool enableHTTPLog = false, string environment = "PROD")
        {
            _environment = environment;
            _certificate = null;
            _callbackUrl = callbackUrl;
            _payeePaymentReference = payeePaymentReference;
            _enableHTTPLog = enableHTTPLog;
        }

        /// <summary>
        /// Construct a Refund client for Swish Payment, with certificate file
        /// </summary>
        /// <param name="certificatePath">Path where to find the .p12 certificate on disc example: c:\cert\swish.p12</param>
        /// <param name="certificatePassword">The password to use the certificate</param>
        /// <param name="callbackUrl">URL where you like to get the Swish Payment Callback</param>
        /// <param name="payeePaymentReference">Payment reference supplied by theMerchant. This is not used by Swish but is included in responses back to the client. This reference could for example be an order id or similar. If set the value must not exceed 35 characters and only the following characters are allowed: [a-ö, A-Ö, 0-9, -]</param>
        /// <param name="enableHTTPLog">Set to true to log HTTP Requests to the Swish Payment API</param>
        /// <param name="environment">Set what environment of Swish Payment API should be used, PROD, SANDBOX or EMULATOR</param>
        public RefundClient(string certificatePath, string certificatePassword, string callbackUrl, string payeePaymentReference, bool enableHTTPLog = false, string environment = "PROD")
        {
            _certificate = new ClientCertificate()
            {
                Path = certificatePath,
                Password = certificatePassword
            };
            _environment = environment;
            _callbackUrl = callbackUrl;
            _payeePaymentReference = payeePaymentReference;
            _enableHTTPLog = enableHTTPLog;
        }
        // Guid.NewGuid().ToString("N").ToUpper()

        /// <summary>
        /// Initiate a Swish Refund Request
        /// </summary>
        /// <param name="originalPaymentReference">Reference of the original payment that this refund is for.</param>
        /// <param name="payerAlias">The Swish number of the merchant that makes the refund payment.</param>
        /// <param name="amount">The amount of money to pay. The amount cannot be less than 0.01 SEK and not more than 999999999999.99 SEK. Valid value has to be all digits or with 2 digit decimal separated with a period.</param>
        /// <param name="message">Merchant supplied message about the payment/order. Max 50 characters. Common allowed characters are the letters a-ö, A-Ö, the numbers 0-9, and special characters !?=#$%&()*+,-./:;<'"@. In addition, the following special characters are also allowed: ^¡¢£€¥¿Š§šŽžŒœŸÀÁÂÃÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕØØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿ.</param>
        /// <param name="instructionUUID">An identifier created by the merchant to uniquely identify a payout instruction sent to the Swish system. Swish uses this identifier to guarantee the uniqueness of a payout instruction and prevent occurrence of unintended double payments. 32 hexadecimal (16- based) digits. Use Guid.NewGuid().ToString("N").ToUpper()</param>
        /// <returns></returns>
        public RefundResponse MakeRefundRequest(string originalPaymentReference, string payerAlias, int amount, string message, string instructionUUID)
        {
            try
            {
                var requestData = new RefundData()
                {
                    originalPaymentReference = originalPaymentReference,
                    payerPaymentReference = _payeePaymentReference,
                    callbackUrl = _callbackUrl,
                    payerAlias = payerAlias,
                    amount = amount.ToString(),
                    currency = "SEK",
                    message = message
                };

                HttpClientHandler handler;
                HttpClient client;
                PrepareHttpClientAndHandler(out handler, out client);

                string requestURL = URL.ProductionRefundRequest + instructionUUID;

                var httpMethod = HttpMethod.Put;

                switch (_environment)
                {
                    case "EMULATOR":
                        requestURL = URL.EmulatorRefundRequest + instructionUUID;
                        break;
                    case "SANDBOX":
                        requestURL = URL.SandboxRefundRequest;
                        httpMethod = HttpMethod.Post;
                        break;
                }

                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = httpMethod,
                    RequestUri = new Uri(requestURL),
                    Content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json")
                };

                httpRequestMessage.Headers.Add("host", httpRequestMessage.RequestUri.Host);

                var response = client.SendAsync(httpRequestMessage).Result;

                string errorMessage = string.Empty;
                string location = string.Empty;

                if (response.IsSuccessStatusCode)
                {
                    var headers = response.Headers.ToList();

                    if (headers.Any(x => x.Key == "Location"))
                    {
                        location = response.Headers.GetValues("Location").FirstOrDefault();
                    }
                }
                else
                {
                    var readAsStringAsync = response.Content.ReadAsStringAsync();
                    errorMessage = readAsStringAsync.Result;
                }

                client.Dispose();
                handler.Dispose();

                return new RefundResponse()
                {
                    Error = errorMessage,
                    Location = location
                };
            }
            catch (Exception ex)
            {
                return new RefundResponse()
                {
                    Error = ex.ToString(),
                    Location = ""
                };
            }
        }

        /// <summary>
        /// Check what the status of the refund is
        /// </summary>
        /// <param name="url">The URL we got from the refund request Location header</param>
        /// <returns></returns>
        public CheckRefundStatusResponse CheckRefundStatus(string url)
        {
            try
            {
                HttpClientHandler handler;
                HttpClient client;
                PrepareHttpClientAndHandler(out handler, out client);
                client.BaseAddress = new Uri(url);


                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Get
                };

                httpRequestMessage.Headers.Add("host", httpRequestMessage.RequestUri.Host);

                var response = client.SendAsync(httpRequestMessage).Result;

                string errorMessage = string.Empty;
                string PaymentRequestToken = string.Empty;
                CheckRefundStatusResponse r = null;

                if (response.IsSuccessStatusCode)
                {
                    var readAsStringAsync = response.Content.ReadAsStringAsync();
                    string jsonResponse = readAsStringAsync.Result;

                    r = JsonConvert.DeserializeObject<CheckRefundStatusResponse>(jsonResponse);
                }
                else
                {
                    var readAsStringAsync = response.Content.ReadAsStringAsync();
                    errorMessage = readAsStringAsync.Result;
                }

                client.Dispose();
                handler.Dispose();

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    return new CheckRefundStatusResponse()
                    {
                        errorCode = "Error",
                        errorMessage = errorMessage
                    };
                }
                else
                {
                    return r;
                }
            }
            catch (Exception ex)
            {
                return new CheckRefundStatusResponse()
                {
                    errorCode = "Exception",
                    errorMessage = ex.ToString()
                };
            }
        }

        private void PrepareHttpClientAndHandler(out HttpClientHandler handler, out HttpClient client)
        {
            handler = new HttpClientHandler();

            if (_certificate != null)
            {
                // Got help for this code on https://stackoverflow.com/questions/61677247/can-a-p12-file-with-ca-certificates-be-used-in-c-sharp-without-importing-them-t
                using (X509Store store = new X509Store(StoreName.CertificateAuthority, StoreLocation.CurrentUser))
                {
                    store.Open(OpenFlags.ReadWrite);

                    var certs = new X509Certificate2Collection();
                    certs.Import(_certificate.Path, _certificate.Password, X509KeyStorageFlags.DefaultKeySet);

                    foreach (X509Certificate2 cert in certs)
                    {
                        if (cert.HasPrivateKey)
                        {
                            handler.ClientCertificates.Add(cert);
                        }
                        else
                        {
                            store.Add(cert);
                        }
                    }
                }
            }

            client = new HttpClient(new LoggingHandler(handler, true));
        }
    }
}

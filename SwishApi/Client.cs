using Newtonsoft.Json;
using SwishApi.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SwishApi
{
    public class Client
    {
        readonly string _baseAPIUrl;
        readonly string _merchantAlias;
        readonly string _callbackUrl;
        readonly string _payeePaymentReference;
        readonly ClientCertificate _certificate;

        class ClientCertificate
        {
            public string Path { get; set; }
            
            public byte[] Content { get; set; }

            public string Password { get; set; }
        }

        public bool EnableHTTPLog { get; set; }

        /// <summary>
        /// This constructor being used for test environment of Swish for Merchant
        /// </summary>
        /// <param name="certificatePath"></param>
        /// <param name="certificatePassword"></param>
        public Client(string certificatePath, string certificatePassword, string callbackUrl) : this(
            new ClientCertificate()
            {
                Path = certificatePath,
                Content = System.IO.File.ReadAllBytes(certificatePath),
                Password = certificatePassword
            },
            "https://mss.cpc.getswish.net", // Test environment
            callbackUrl,
            "01234679304",
            "1234679304"
        ) {}

        public Client(string certificatePath, string certificatePassword, string callbackUrl, string payeePaymentReference, string merchantAlias) : this(
            new ClientCertificate()
            {
                Path = certificatePath,
                Content = System.IO.File.ReadAllBytes(certificatePath),
                Password = certificatePassword
            },
            "https://cpc.getswish.net", // Live environment
            callbackUrl,
            payeePaymentReference,
            merchantAlias
        ) {}

        public Client(string callbackUrl, string baseUrl = "https://mss.cpc.getswish.net") : this
        (
            certificate: null,
            baseUrl: baseUrl,
            callbackUrl: callbackUrl,
            payeePaymentReference: "01234679304",
            merchantAlias: "1234679304"
        ) {}

        private Client(ClientCertificate certificate, string baseUrl, string callbackUrl, string payeePaymentReference, string merchantAlias)
        {
            _certificate = certificate;
            _baseAPIUrl = baseUrl;
            _callbackUrl = callbackUrl;
            _merchantAlias = merchantAlias;
            _payeePaymentReference = payeePaymentReference;
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

            var baseAddress = new Uri(_baseAPIUrl);

            //client = new HttpClient(handler) { BaseAddress = baseAddress };
            client = new HttpClient(new LoggingHandler(handler, EnableHTTPLog));
        }

        public PayoutRequestResponse MakePayoutRequest(string payoutUUID, string phonenumber, string payeeSSN, string amount, string message, string signingCertificateSerialNumber, string signingCertificatePath = null, string signingCertificatePassword = null)
        {
            try
            {
                var requestEnvelope = new PayoutRequestEnvelope()
                {
                    payload = new PayoutRequestData()
                    {
                        payoutInstructionUUID = payoutUUID,
                        payerPaymentReference = _payeePaymentReference,
                        payerAlias = _merchantAlias,
                        payeeAlias = phonenumber,
                        payeeSSN = payeeSSN,
                        amount = amount,
                        currency = "SEK",
                        payoutType = "PAYOUT",
                        message = message,
                        instructionDate = DateTime.Now.ToString("s"),
                        signingCertificateSerialNumber = signingCertificateSerialNumber
                    },
                    callbackUrl = _callbackUrl
                };
                requestEnvelope.buildSignature(signingCertificatePath, signingCertificatePassword);

                HttpClientHandler handler;
                HttpClient client;
                PrepareHttpClientAndHandler(out handler, out client);

                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(_baseAPIUrl + "/swish-cpcapi/api/v1/payouts"),
                    Content = new StringContent(JsonConvert.SerializeObject(requestEnvelope), Encoding.UTF8, "application/json")
                };

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
                    errorMessage = response.StatusCode + "|" + readAsStringAsync.Result;
                    
                }

                client.Dispose();
                handler.Dispose();

                return new PayoutRequestResponse()
                {
                    Error = errorMessage,
                    Location = location,
                    JSON = JsonConvert.SerializeObject(requestEnvelope)
                };
            }
            catch (Exception ex)
            {
                return new PayoutRequestResponse()
                {
                    Error = ex.ToString()
                };
            }
        }

        public PaymentRequestECommerceResponse MakePaymentRequest(string phonenumber, int amount, string message)
        {
            try
            {
                var requestData = new PaymentRequestECommerceData()
                {
                    payeePaymentReference = _payeePaymentReference,
                    callbackUrl = _callbackUrl,
                    payerAlias = phonenumber,
                    payeeAlias = _merchantAlias,
                    amount = amount.ToString(),
                    currency = "SEK",
                    message = message
                };

                HttpClientHandler handler;
                HttpClient client;
                PrepareHttpClientAndHandler(out handler, out client);

                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(_baseAPIUrl + "/swish-cpcapi/api/v1/paymentrequests"),
                    Content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json")
                };

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

                return new PaymentRequestECommerceResponse()
                {
                    Error = errorMessage,
                    Location = location
                };
            }
            catch (Exception ex)
            {
                return new PaymentRequestECommerceResponse()
                {
                    Error = ex.ToString(),
                    Location = ""
                };
            }
        }

        public PaymentRequestMCommerceResponse MakePaymentRequestMCommerce(int amount, string message)
        {
            try
            {
                var requestData = new PaymentRequestMCommerceData()
                {
                    payeePaymentReference = _payeePaymentReference,
                    callbackUrl = _callbackUrl,
                    payeeAlias = _merchantAlias,
                    amount = amount.ToString(),
                    currency = "SEK",
                    message = message
                };

                HttpClientHandler handler;
                HttpClient client;
                PrepareHttpClientAndHandler(out handler, out client);

                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(_baseAPIUrl + "/swish-cpcapi/api/v1/paymentrequests"),
                    Content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json")
                };

                var response = client.SendAsync(httpRequestMessage).Result;

                string errorMessage = string.Empty;
                string PaymentRequestToken = string.Empty;
                string Location = string.Empty;

                if (response.IsSuccessStatusCode)
                {
                    var headers = response.Headers.ToList();

                    if (headers.Any(x => x.Key == "PaymentRequestToken"))
                    {
                        PaymentRequestToken = response.Headers.GetValues("PaymentRequestToken").FirstOrDefault();
                    }

                    if (headers.Any(x => x.Key == "Location"))
                    {
                        Location = response.Headers.GetValues("Location").FirstOrDefault();
                    }
                }
                else
                {
                    var readAsStringAsync = response.Content.ReadAsStringAsync();
                    errorMessage = readAsStringAsync.Result;
                }

                client.Dispose();
                handler.Dispose();

                return new PaymentRequestMCommerceResponse()
                {
                    Error = errorMessage,
                    Token = PaymentRequestToken, 
                    Location = Location
                };
            }
            catch (Exception ex)
            {
                return new PaymentRequestMCommerceResponse()
                {
                    Error = ex.ToString(),
                    Token = "",
                    Location = ""
                };
            }
        }

        public CheckPaymentRequestStatusResponse CheckPaymentStatus(string url)
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

                var response = client.SendAsync(httpRequestMessage).Result;

                string errorMessage = string.Empty;
                string PaymentRequestToken = string.Empty;
                CheckPaymentRequestStatusResponse r = null;

                if (response.IsSuccessStatusCode)
                {
                    var readAsStringAsync = response.Content.ReadAsStringAsync();
                    string jsonResponse = readAsStringAsync.Result;

                    r = JsonConvert.DeserializeObject<CheckPaymentRequestStatusResponse>(jsonResponse);
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
                    return new CheckPaymentRequestStatusResponse()
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
                return new CheckPaymentRequestStatusResponse()
                {
                    errorCode = "Exception",
                    errorMessage = ex.ToString()
                };
            }
        }

        public CheckPayoutRequestStatusResponse CheckPayoutStatus(string url)
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

                var response = client.SendAsync(httpRequestMessage).Result;

                string errorMessage = string.Empty;
                string PaymentRequestToken = string.Empty;
                CheckPayoutRequestStatusResponse r = null;

                if (response.IsSuccessStatusCode)
                {
                    var readAsStringAsync = response.Content.ReadAsStringAsync();
                    string jsonResponse = readAsStringAsync.Result;

                    r = JsonConvert.DeserializeObject<CheckPayoutRequestStatusResponse>(jsonResponse);
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
                    return new CheckPayoutRequestStatusResponse()
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
                return new CheckPayoutRequestStatusResponse()
                {
                    errorCode = "Exception",
                    errorMessage = ex.ToString()
                };
            }
        }

        public RefundResponse Refund(string originalPaymentReference, double amount, string message, string refundCallbackUrl)
        {
            try
            {
                var requestData = new RefundData()
                {
                    originalPaymentReference = originalPaymentReference,
                    callbackUrl = refundCallbackUrl,
                    payerAlias = _merchantAlias,
                    amount = amount.ToString(),
                    currency = "SEK",
                    message = message
                };

                HttpClientHandler handler;
                HttpClient client;
                PrepareHttpClientAndHandler(out handler, out client);

                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(_baseAPIUrl + "/swish-cpcapi/api/v1/refunds"),
                    Content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json")
                };

                var response = client.SendAsync(httpRequestMessage).Result;

                string errorMessage = string.Empty;
                string Location = string.Empty;

                if (response.IsSuccessStatusCode)
                {
                    var headers = response.Headers.ToList();

                    if (headers.Any(x => x.Key == "Location"))
                    {
                        Location = response.Headers.GetValues("Location").FirstOrDefault();
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
                    Location = Location
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
        public QRCodeResponse GetQRCode(string token, string format = "svg", int size = 300, int border = 0, bool transparent = true)
        {
            try
            {
                var requestData = new QRCodeData()
                {
                    token = token,
                    format = format,
                    size = size,
                    border = border,
                    transparent = transparent
                };

                HttpClientHandler handler = new HttpClientHandler();
                HttpClient client = new HttpClient(handler) { BaseAddress = new Uri("https://mpc.getswish.net") };

                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("https://mpc.getswish.net/qrg-swish/api/v1/commerce"),
                    Content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json")
                };

                var response = client.SendAsync(httpRequestMessage).Result;

                string errorMessage = string.Empty;
                string svgData = string.Empty;

                if (response.IsSuccessStatusCode)
                {
                    var readAsStringAsync = response.Content.ReadAsStringAsync();
                    svgData = readAsStringAsync.Result;
                }
                else
                {
                    var readAsStringAsync = response.Content.ReadAsStringAsync();
                    errorMessage = readAsStringAsync.Result;
                }

                client.Dispose();
                handler.Dispose();

                return new QRCodeResponse()
                {
                    Error = errorMessage,
                    SVGData = svgData
                };
            }
            catch (Exception ex)
            {
                return new QRCodeResponse()
                {
                    Error = ex.ToString(),
                    SVGData = string.Empty
                };
            }
        }
    }
}

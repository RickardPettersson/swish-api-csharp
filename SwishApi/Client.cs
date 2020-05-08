using Newtonsoft.Json;
using RestSharp;
using SwishApi.Models;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SwishApi
{
    public class Client
    {
        public string _certificatePath;
        public string _certificatePassword;
        public string _baseAPIUrl;
        public string _payeeAlias;
        public byte[] _certDataBytes;
        public string _callbackUrl;
        public string _payeePaymentReference;

        /// <summary>
        /// This constructor being used for test environment of Swish for Merchant
        /// </summary>
        /// <param name="certificatePath"></param>
        /// <param name="certificatePassword"></param>
        public Client(string certificatePath, string certificatePassword, string callbackUrl)
        {
            _certificatePath = certificatePath;
            _certDataBytes = System.IO.File.ReadAllBytes(certificatePath);
            _certificatePassword = certificatePassword;
            _baseAPIUrl = "https://mss.cpc.getswish.net"; // Test environment
            _payeeAlias = "1234679304";
            _callbackUrl = callbackUrl;
            _payeePaymentReference = "01234679304";
        }


        public Client(string certificatePath, string certificatePassword, string callbackUrl, string payeePaymentReference, string payeeAlias)
        {
            _certificatePath = certificatePath;
            _certDataBytes = System.IO.File.ReadAllBytes(certificatePath);
            _certificatePassword = certificatePassword;
            _baseAPIUrl = "https://cpc.getswish.net"; // Live environment
            _payeeAlias = payeeAlias;
            _callbackUrl = callbackUrl;
            _payeePaymentReference = payeePaymentReference;
        }

        private void PrepareHttpClientAndHandler(out HttpClientHandler handler, out HttpClient client)
        {
            // Got help for this code on https://stackoverflow.com/questions/61677247/can-a-p12-file-with-ca-certificates-be-used-in-c-sharp-without-importing-them-t
            handler = new HttpClientHandler();
            using (X509Store store = new X509Store(StoreName.CertificateAuthority, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadWrite);

                var certs = new X509Certificate2Collection();
                certs.Import(_certificatePath, _certificatePassword, X509KeyStorageFlags.DefaultKeySet);

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

            var baseAddress = new Uri(_baseAPIUrl);

            client = new HttpClient(handler) { BaseAddress = baseAddress };
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
                    payeeAlias = _payeeAlias,
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
                    payeeAlias = _payeeAlias,
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

                if (response.IsSuccessStatusCode)
                {
                    var headers = response.Headers.ToList();

                    if (headers.Any(x => x.Key == "PaymentRequestToken"))
                    {
                        PaymentRequestToken = response.Headers.GetValues("PaymentRequestToken").FirstOrDefault();
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
                    Token = PaymentRequestToken
                };
            }
            catch (Exception ex)
            {
                return new PaymentRequestMCommerceResponse()
                {
                    Error = ex.ToString(),
                    Token = ""
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
                } else
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

        public RefundResponse Refund(string originalPaymentReference, double amount, string message, string refundCallbackUrl)
        {
            try
            {
                var requestData = new RefundData()
                {
                    originalPaymentReference = originalPaymentReference,
                    callbackUrl = refundCallbackUrl,
                    payerAlias = _payeeAlias,
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

        public CheckRefundStatusResponse CheckRefundStatus2(string url)
        {
            try
            {
                // Create a RestSharp RestClient objhect with the base URL
                var client = new RestClient(url);

                // Create a request object with the path to the payment requests
                var request = new RestRequest();

                // Create up a client certificate collection and import the certificate to it
                X509Certificate2Collection clientCertificates = new X509Certificate2Collection();
                clientCertificates.Import(_certDataBytes, _certificatePassword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);

                // Add client certificate collection to the RestClient
                client.ClientCertificates = clientCertificates;
                
                var response = client.Get<CheckRefundStatusResponse>(request);

                if (response.ErrorException != null)
                {
                    return new CheckRefundStatusResponse()
                    {
                        errorCode = "ERROR",
                        errorMessage = response.ErrorException.ToString()
                    };
                }
                else
                {
                    return response.Data;
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

        public QRCodeResponse GetQRCode(string token, string format = "png", int size = 300, int border = 0, bool transparent = true)
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

                // Create a RestSharp RestClient objhect with the base URL
                var client = new RestClient("https://mpc.getswish.net");

                // Create a request object with the path to the payment requests
                var request = new RestRequest("qrg-swish/api/v1/commerce");
                
                // Add payment request data
                request.AddJsonBody(requestData);

                var data = client.Post<QRCodeData>(request);

                //var data = client.DownloadData(request);
                
                return new QRCodeResponse()
                {
                    Error = "",
                    Data = data.RawBytes
                };
            }
            catch (Exception ex)
            {
                return new QRCodeResponse()
                {
                    Error = ex.ToString()
                };
            }
        }
    }
}

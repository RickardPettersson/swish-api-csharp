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
    public class PayoutClient
    {
        readonly string _environment;
        readonly string _merchantAlias;
        readonly string _callbackUrl;
        readonly string _payeePaymentReference;
        readonly ClientCertificate _certificate;
        readonly bool _enableHTTPLog;

        /// <summary>
        /// Construct a Payout client for Swish Payout, with certificate file
        /// </summary>
        /// <param name="certificatePath">Path where to find the .p12 certificate on disc example: c:\cert\swish.p12</param>
        /// <param name="certificatePassword">The password to use the certificate</param>
        /// <param name="callbackUrl">URL where you like to get the Swish Payment Callback</param>
        /// <param name="payeePaymentReference">Payment reference supplied by theMerchant. This is not used by Swish but is included in responses back to the client. This reference could for example be an order id or similar. If set the value must not exceed 35 characters and only the following characters are allowed: [a-ö, A-Ö, 0-9, -]</param>
        /// <param name="payeeAlias">The Swish number of the payee. It needs to match with Merchant Swish number.</param>
        /// <param name="enableHTTPLog">Set to true to log HTTP Requests to the Swish Payment API</param>
        /// <param name="environment">Set what environment of Swish Payment API should be used, PROD, SANDBOX or EMULATOR</param>
        public PayoutClient(string certificatePath, string certificatePassword, string callbackUrl, string payeePaymentReference, string merchantAlias, bool enableHTTPLog = false, string environment = "PROD")
        {
            _certificate = new ClientCertificate()
            {
                CertificateFilePath = certificatePath,
                Password = certificatePassword
            };
            _environment = environment;
            _callbackUrl = callbackUrl;
            _merchantAlias = merchantAlias;
            _payeePaymentReference = payeePaymentReference;
            _enableHTTPLog = enableHTTPLog;
        }

        /// <summary>
        /// Construct a payout client for Swish Payout, with certificate file
        /// </summary>
        /// <param name="clientCertificate">Client Certificate object</param>
        /// <param name="callbackUrl">URL where you like to get the Swish Payment Callback</param>
        /// <param name="payeePaymentReference">Payment reference supplied by theMerchant. This is not used by Swish but is included in responses back to the client. This reference could for example be an order id or similar. If set the value must not exceed 35 characters and only the following characters are allowed: [a-ö, A-Ö, 0-9, -]</param>
        /// <param name="payeeAlias">The Swish number of the payee. It needs to match with Merchant Swish number.</param>
        /// <param name="enableHTTPLog">Set to true to log HTTP Requests to the Swish Payment API</param>
        /// <param name="environment">Set what environment of Swish Payment API should be used, PROD, SANDBOX or EMULATOR</param>
        public PayoutClient(ClientCertificate clientCertificate, string callbackUrl, string payeePaymentReference, string merchantAlias, bool enableHTTPLog = false, string environment = "PROD")
        {
            _certificate = clientCertificate;
            _environment = environment;
            _callbackUrl = callbackUrl;
            _merchantAlias = merchantAlias;
            _payeePaymentReference = payeePaymentReference;
            _enableHTTPLog = enableHTTPLog;
        }

        /// <summary>
        /// Initiate a Swish Payment Request
        /// </summary>
        /// <param name="payoutTo">The Swish number of where the payout should be send. No preceding “+” or zeros should be added. It should always be started with country code. Numeric, 8-15 digits</param>
        /// <param name="personalNumber">12 digit personal number for the receiver, this will be validated agains the mobile number the payout going to payed to. Formated as YYYYMMDDnnnn</param>
        /// <param name="amount">The amount of money to pay. The amount cannot be less than 0.01 SEK and not more than 999999999999.99 SEK. Valid value has to be all digits or with 2 digit decimal separated with a period.</param>
        /// <param name="message">Merchant supplied message about the payment/order. Max 50 characters. Common allowed characters are the letters a-ö, A-Ö, the numbers 0-9, and special characters !?=#$%&()*+,-./:;<'"@. In addition, the following special characters are also allowed: ^¡¢£€¥¿Š§šŽžŒœŸÀÁÂÃÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕØØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿ.</param>
        /// <param name="instructionUUID">An identifier created by the merchant to uniquely identify a payout instruction sent to the Swish system. Swish uses this identifier to guarantee the uniqueness of a payout instruction and prevent occurrence of unintended double payments. 32 hexadecimal (16- based) digits. Use Guid.NewGuid().ToString("N").ToUpper()</param>
        /// <param name="signingCertificateSerialNumber">The public key of the certificate will be used to verify the signature. Formated as the serial number of the certificate in hexadecimal format (without the leading ‘0x’). Max length 64 digits.</param>
        /// <param name="signingCertificate">Put in the certificate path and things for signing the payout message, if this paramter is null its using the signingCertificateSerialNumber</param>
        /// <returns></returns>
        public PayoutRequestResponse MakePayoutRequest(string payoutTo, string personalNumber, decimal amount, string message, string instructionUUID, string signingCertificateSerialNumber, ClientCertificate signingCertificate)
        {
            PayoutRequestData payload = null;

            try
            {
                var requestEnvelope = new PayoutRequestEnvelope()
                {
                    payload = new PayoutRequestData()
                    {
                        payoutInstructionUUID = instructionUUID,
                        payerPaymentReference = _payeePaymentReference,
                        payerAlias = _merchantAlias, // On payout the payer is the merchant swish number
                        payeeAlias = payoutTo, // On payout the payee is the number where the payout going to be send
                        payeeSSN = personalNumber,
                        amount = Math.Round(amount, 2).ToString().Replace(",", "."), // Amount to be paid. Only period/dot (”.”) are accepted as decimal character with maximum 2 digits after. Digits after separator are optional.
                        currency = "SEK",
                        payoutType = "PAYOUT",
                        message = message,
                        instructionDate = DateTime.Now.ToString("s"),
                        signingCertificateSerialNumber = signingCertificateSerialNumber
                    },
                    callbackUrl = _callbackUrl
                };
                requestEnvelope.buildSignature(signingCertificate);

                payload = requestEnvelope.payload;

                HttpClientHandler handler;
                HttpClient client;
                PrepareHttpClientAndHandler(out handler, out client);

                string requestURL = URL.ProductionPayoutRequest;

                switch (_environment)
                {
                    case "EMULATOR":
                        requestURL = URL.EmulatorPayoutRequest;
                        break;
                    case "SANDBOX":
                        requestURL = URL.SandboxPayoutRequest;
                        break;
                }

                string json = JsonConvert.SerializeObject(requestEnvelope);

                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(requestURL),
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
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

                return new PayoutRequestResponse()
                {
                    Error = errorMessage,
                    Location = location,
                    Payload = payload,
                    JSON = "" // Old stuff to remove after removed Client.cs
                };
            }
            catch (Exception ex)
            {
                return new PayoutRequestResponse()
                {
                    Error = ex.ToString(),
                    Location = "",
                    Payload = payload,
                    JSON = "" // Old stuff to remove after removed Client.cs
                };
            }
        }

        /// <summary>
        /// Check what the status of the payment is
        /// </summary>
        /// <param name="url">The URL we got from the payment request Location header</param>
        /// <returns></returns>
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

                httpRequestMessage.Headers.Add("host", client.BaseAddress.Host);

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

        private void PrepareHttpClientAndHandler(out HttpClientHandler handler, out HttpClient client)
        {
            handler = new HttpClientHandler();

            if (_certificate != null)
            {
                if (_certificate.UseMachineKeySet)
                {
                    if (string.IsNullOrEmpty(_certificate.Password))
                    {
                        if (_certificate.SecureStringPassword != null)
                        {
                            var cert = new X509Certificate2(Misc.ReadFully(_certificate.CertificateAsStream), _certificate.SecureStringPassword, X509KeyStorageFlags.MachineKeySet);

                            handler.ClientCertificates.Add(cert);
                        }
                        else
                        {
                            throw new Exception("Certificate password missing set wish needed to use with MachineKeySet");
                        }
                    }
                    else
                    {
                        var cert = new X509Certificate2(Misc.ReadFully(_certificate.CertificateAsStream), _certificate.Password, X509KeyStorageFlags.MachineKeySet);

                        handler.ClientCertificates.Add(cert);
                    }
                }
                else
                {
                    // Got help for this code on https://stackoverflow.com/questions/61677247/can-a-p12-file-with-ca-certificates-be-used-in-c-sharp-without-importing-them-t
                    using (X509Store store = new X509Store(StoreName.CertificateAuthority, StoreLocation.CurrentUser))
                    {
                        store.Open(OpenFlags.ReadWrite);

                        if (string.IsNullOrEmpty(_certificate.CertificateFilePath))
                        {
                            if (string.IsNullOrEmpty(_certificate.Password))
                            {
                                var cert = new X509Certificate2(Misc.ReadFully(_certificate.CertificateAsStream));

                                if (cert.HasPrivateKey)
                                {
                                    handler.ClientCertificates.Add(cert);
                                }
                                else
                                {
                                    store.Add(cert);
                                }
                            }
                            else
                            {
                                var cert = new X509Certificate2(Misc.ReadFully(_certificate.CertificateAsStream), _certificate.Password);

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
                        else
                        {
                            var certs = new X509Certificate2Collection();

                            certs.Import(_certificate.CertificateFilePath, _certificate.Password, X509KeyStorageFlags.DefaultKeySet);

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
                }
            }

            client = new HttpClient(new LoggingHandler(handler, _enableHTTPLog));
        }
    }
}

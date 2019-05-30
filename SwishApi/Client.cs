using RestSharp;
using SwishApi.Models;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

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
            _payeeAlias = "1231181189";
            _callbackUrl = callbackUrl;
            _payeePaymentReference = "01231181189";
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

                // Create a RestSharp RestClient objhect with the base URL
                var client = new RestClient(_baseAPIUrl);

                // Create a request object with the path to the payment requests
                var request = new RestRequest("swish-cpcapi/api/v1/paymentrequests");

                // Create up a client certificate collection and import the certificate to it
                X509Certificate2Collection clientCertificates = new X509Certificate2Collection();
                clientCertificates.Import(_certDataBytes, _certificatePassword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);

                // Add client certificate collection to the RestClient
                client.ClientCertificates = clientCertificates;

                // Add payment request data
                request.AddJsonBody(requestData);

                var response = client.Post(request);
                var content = response.Content;

                if (response.ErrorException != null)
                {
                    return new PaymentRequestECommerceResponse()
                    {
                        Error = response.ErrorException.ToString(),
                        Location = ""
                    };
                }
                else
                {

                    string location = response.Headers.ToList().Find(x => x.Name == "Location").Value.ToString();

                    return new PaymentRequestECommerceResponse()
                    {
                        Error = "",
                        Location = location
                    };
                }
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

        public CheckPaymentRequestStatusResponse CheckPaymentStatus(string url)
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

                var response = client.Get<CheckPaymentRequestStatusResponse>(request);

                if (response.ErrorException != null)
                {
                    return new CheckPaymentRequestStatusResponse()
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
                return new CheckPaymentRequestStatusResponse()
                {
                    errorCode = "Exception",
                    errorMessage = ex.ToString()
                };
            }
        }

        public RefundResponse Refund(string originalPaymentReference, double amount, string message)
        {
            try
            {
                var refundData = new RefundData()
                {
                    originalPaymentReference = originalPaymentReference,
                    callbackUrl = _callbackUrl,
                    payerAlias = _payeeAlias,
                    amount = amount.ToString(),
                    currency = "SEK",
                    message = message
                };

                // Create a RestSharp RestClient objhect with the base URL
                var client = new RestClient(_baseAPIUrl);

                // Create a request object with the path to the payment requests
                var request = new RestRequest("swish-cpcapi/api/v1/refunds");

                // Create up a client certificate collection and import the certificate to it
                X509Certificate2Collection clientCertificates = new X509Certificate2Collection();
                clientCertificates.Import(_certDataBytes, _certificatePassword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);

                // Add client certificate collection to the RestClient
                client.ClientCertificates = clientCertificates;

                // Add payment request data
                request.AddJsonBody(refundData);

                var response = client.Post(request);
                var content = response.Content;

                // "[{\"errorCode\":\"PA01\",\"errorMessage\":null,\"additionalInformation\":null}]"

                if (!response.IsSuccessful)
                {
                    return new RefundResponse()
                    {
                        Error = content,
                        Location = ""
                    };
                }
                else
                {
                    string location = response.Headers.ToList().Find(x => x.Name == "Location").Value.ToString();

                    return new RefundResponse()
                    {
                        Error = "",
                        Location = location
                    };
                }
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
    }
}

using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

namespace SwishApiConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //MainTestPayment(args);
            //MainTestQCommerce(args);
            MainTestPayout(args);
        }

        // MainTestPaymentAndRefund
        static void MainTestPayment(string[] args)
        {
            // Get the path for the test certificate in the TestCert folder in the console application folder, being always copy to the output folder
            string certificatePath = Environment.CurrentDirectory + "\\TestCert\\Swish_Merchant_TestCertificate_1234679304.p12";

            // Create a Swishpi Client object with all data needed to run a test Swish payment
            SwishApi.Client client = new SwishApi.Client(certificatePath, "swish", "https://tabetaltmedswish.se/Test/Callback/");

            // Make the Payement Request
            var response = client.MakePaymentRequest("1234679304", 1, "Test");//46731596605

            // Check if the payment request got success and not got any error
            if (string.IsNullOrEmpty(response.Error))
            {
                // All OK
                string urlForCheckingPaymentStatus = response.Location;

                // If you do a webbapplication you here should wait some time, showing a "loading" view or something and try to do the payment status check as below, you maybe have some ajax request doing a call to a actionresult doing this code
                // Wait so that the payment request has been processed
                System.Threading.Thread.Sleep(5000);

                // Make the payment status check
                var statusResponse = client.CheckPaymentStatus(urlForCheckingPaymentStatus);

                // Check if the call is done correct
                if (string.IsNullOrEmpty(statusResponse.errorCode))
                {
                    // Call was maked without any problem
                    Console.WriteLine("Status: " + statusResponse.status);

                    if (statusResponse.status == "PAID")
                    {
                        // "8FFBC84A91CD49A799176B1419AAE598"
                        var refundResponse = client.Refund(statusResponse.paymentReference, statusResponse.amount, "Återköp", "https://tabetaltmedswish.se/Test/RefundCallback/");

                        if (string.IsNullOrEmpty(refundResponse.Error))
                        {
                            // Request OK
                            string urlForCheckingRefundStatus = refundResponse.Location;

                            // If you do a webbapplication you here should wait some time, showing a "loading" view or something and try to do the refund status check as below, you maybe have some ajax request doing a call to a actionresult doing this code
                            // Wait so that the refund has been processed
                            System.Threading.Thread.Sleep(5000);

                            // Check refund status
                            var refundCheckResposne = client.CheckRefundStatus(urlForCheckingRefundStatus);

                            if (string.IsNullOrEmpty(refundCheckResposne.errorCode))
                            {
                                // Call was maked without any problem
                                Console.WriteLine("RefundChecKResponse - Status: " + statusResponse.status);
                            }
                            else
                            {
                                // ERROR
                                Console.WriteLine("RefundCheckResponse: " + refundCheckResposne.errorCode + " - " + refundCheckResposne.errorMessage);
                            }
                        }
                        else
                        {
                            // ERROR
                            Console.WriteLine("Refund Error: " + refundResponse.Error);
                        }
                    }
                }
                else
                {
                    // ERROR
                    Console.WriteLine("CheckPaymentResponse: " + statusResponse.errorCode + " - " + statusResponse.errorMessage);
                }
            }
            else
            {
                // ERROR
                Console.WriteLine("MakePaymentRequest - ERROR: " + response.Error);
            }


            Console.WriteLine(">>> Press enter to exit <<<");
            Console.ReadLine();
        }

        // MainTestQCommerce
        static void MainTestQCommerce(string[] args)
        {
            // Get the path for the test certificate in the TestCert folder in the console application folder, being always copy to the output folder
            string certificatePath = Environment.CurrentDirectory + "\\TestCert\\Swish_Merchant_TestCertificate_1234679304.p12";

            // Create a Swishpi Client object with all data needed to run a test Swish payment
            SwishApi.Client client = new SwishApi.Client(certificatePath, "swish", "https://tabetaltmedswish.se/Test/Callback/");

            var responseMCommerce = client.MakePaymentRequestMCommerce(1, "Test");

            var getQRCodeResponse = client.GetQRCode(responseMCommerce.Token, "svg");

            if (string.IsNullOrEmpty(getQRCodeResponse.Error))
            {
                System.IO.File.WriteAllText("test.svg", getQRCodeResponse.SVGData);

                // If you do a webbapplication you here should wait some time, showing a "loading" view or something and try to do the payment status check as below, you maybe have some ajax request doing a call to a actionresult doing this code
                // Wait so that the payment request has been processed
                System.Threading.Thread.Sleep(5000);

                // Make the payment status check
                var statusResponse = client.CheckPaymentStatus(responseMCommerce.Location);

                // Check if the call is done correct
                if (string.IsNullOrEmpty(statusResponse.errorCode))
                {
                    // Call was maked without any problem
                    Console.WriteLine("Status: " + statusResponse.status);

                    if (statusResponse.status == "PAID")
                    {
                        // Refund the payment
                        var refundResponse = client.Refund(statusResponse.paymentReference, statusResponse.amount, "Återköp", "https://tabetaltmedswish.se/Test/RefundCallback/");

                        if (string.IsNullOrEmpty(refundResponse.Error))
                        {
                            // Request OK
                            string urlForCheckingRefundStatus = refundResponse.Location;

                            // If you do a webbapplication you here should wait some time, showing a "loading" view or something and try to do the refund status check as below, you maybe have some ajax request doing a call to a actionresult doing this code
                            // Wait so that the refund has been processed
                            System.Threading.Thread.Sleep(5000);

                            // Check refund status
                            var refundCheckResposne = client.CheckRefundStatus(urlForCheckingRefundStatus);

                            if (string.IsNullOrEmpty(refundCheckResposne.errorCode))
                            {
                                // Call was maked without any problem
                                Console.WriteLine("RefundChecKResponse - Status: " + statusResponse.status);
                            }
                            else
                            {
                                // ERROR
                                Console.WriteLine("RefundCheckResponse: " + refundCheckResposne.errorCode + " - " + refundCheckResposne.errorMessage);
                            }
                        }
                        else
                        {
                            // ERROR
                            Console.WriteLine("Refund Error: " + refundResponse.Error);
                        }
                    }
                }
                else
                {
                    // ERROR
                    Console.WriteLine("CheckPaymentResponse: " + statusResponse.errorCode + " - " + statusResponse.errorMessage);
                }
            }
            else
            {
                Console.WriteLine("ERROR Get QR Code: " + getQRCodeResponse.Error);
            }

            Console.WriteLine(">>> Press enter to exit <<<");
            Console.ReadLine();
        }

        static void MainTestPayout(string[] args)
        {
            // Get the path for the test certificate in the TestCert folder in the console application folder, being always copy to the output folder
            string certificatePath = Environment.CurrentDirectory + "\\TestCert\\Swish_Merchant_TestCertificate_1234679304.p12";

            // Create a Swishpi Client object with all data needed to run a test Swish payment
            SwishApi.Client client = new SwishApi.Client(certificatePath, "swish", "https://tabetaltmedswish.se/Test/Callback/");
            // client.EnableHTTPLog = true;

            var response = client.MakePayoutRequest(Guid.NewGuid().ToString("N").ToUpper(), "1234679304", "199001019999", "1.00", "Test", "7d70445ec8ef4d1e3a713427e973d097");

            if (string.IsNullOrEmpty(response.Error))
            {
                Console.WriteLine("Location: " + response.Location);
            }
            else
            {
                // ERROR
                Console.WriteLine("MakePaymentRequest - ERROR: " + response.Error);
            }

            Console.WriteLine(">>> Press enter to exit <<<");
            Console.ReadLine();
        }
    }
}

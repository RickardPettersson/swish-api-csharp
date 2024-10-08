﻿using System;
using System.IO;

namespace SwishApiConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
             * Uncomment what function you like to test
             */
            MainTestPayment();
            //MainTestQCommerce();
            //MainTestCancelPayment();
            //MainTestPayout();
        }

        // MainTestPaymentAndRefund
        static void MainTestPayment()
        {
            var clientCertificate = new SwishApi.Models.ClientCertificate()
            {
                //CertificateAsStream = System.IO.File.OpenRead("TestCert//Swish_Merchant_TestCertificate_1234679304.p12"),
                CertificateFilePath = "TestCert//Swish_Merchant_TestCertificate2_1234679304.p12",
                Password = "swish",
                UseMachineKeySet = true
            };

            var eCommerceClient = new SwishApi.ECommerceClient(clientCertificate, "https://eo486cwao01o9rl.m.pipedream.net", "12345", "1234679304", true, SwishApi.Environment.Emulator);

            string instructionUUID = Guid.NewGuid().ToString("N").ToUpper();

            // Make the Payement Request
            var response = eCommerceClient.MakePaymentRequest("1234679304", 1, "Test", instructionUUID);

            // Check if the payment request got success and not got any error
            if (string.IsNullOrEmpty(response.Error))
            {
                // All OK
                string urlForCheckingPaymentStatus = response.Location;

                // If you do a webbapplication you here should wait some time, showing a "loading" view or something and try to do the payment status check as below, you maybe have some ajax request doing a call to a actionresult doing this code
                // Wait so that the payment request has been processed
                System.Threading.Thread.Sleep(5000);

                // Make the payment status check
                var statusResponse = eCommerceClient.CheckPaymentStatus(urlForCheckingPaymentStatus);

                // Check if the call is done correct
                if (string.IsNullOrEmpty(statusResponse.errorCode))
                {
                    // Call was maked without any problem
                    Console.WriteLine("Status: " + statusResponse.status);

                    if (statusResponse.status == "PAID")
                    {
                        var refundClient = new SwishApi.RefundClient(clientCertificate, "https://eofvqci6optquip.m.pipedream.net", "1234", "1234679304", true, SwishApi.Environment.Emulator);
                        
                        string instructionUUID2 = Guid.NewGuid().ToString("N").ToUpper();

                        var refundResponse = refundClient.MakeRefundRequest(statusResponse.paymentReference, "0731596605", (int)statusResponse.amount, "Återköp", instructionUUID2);

                        if (string.IsNullOrEmpty(refundResponse.Error))
                        {
                            // Request OK
                            string urlForCheckingRefundStatus = refundResponse.Location;

                            // If you do a webbapplication you here should wait some time, showing a "loading" view or something and try to do the refund status check as below, you maybe have some ajax request doing a call to a actionresult doing this code
                            // Wait so that the refund has been processed
                            System.Threading.Thread.Sleep(5000);

                            // Check refund status
                            var refundCheckResposne = refundClient.CheckRefundStatus(urlForCheckingRefundStatus);

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
        static void MainTestQCommerce()
        {
            var clientCertificate = new SwishApi.Models.ClientCertificate()
            {
                CertificateFilePath = "TestCert//Swish_Merchant_TestCertificate_1234679304.p12",
                Password = "swish"
            };

            var mCommerceClient = new SwishApi.MCommerceClient(clientCertificate, "https://eo486cwao01o9rl.m.pipedream.net", "12345", "1234679304", true, SwishApi.Environment.Emulator);

            string instructionUUID = Guid.NewGuid().ToString("N").ToUpper();

            var responseMCommerce = mCommerceClient.MakePaymentRequest(1, "Test", instructionUUID);

            var getQRCodeResponse = mCommerceClient.GetQRCode(responseMCommerce.Token, "svg");

            if (string.IsNullOrEmpty(getQRCodeResponse.Error))
            {
                System.IO.File.WriteAllText("test.svg", getQRCodeResponse.SVGData);

                // If you do a webbapplication you here should wait some time, showing a "loading" view or something and try to do the payment status check as below, you maybe have some ajax request doing a call to a actionresult doing this code
                // Wait so that the payment request has been processed
                System.Threading.Thread.Sleep(5000);

                // Make the payment status check
                var statusResponse = mCommerceClient.CheckPaymentStatus(responseMCommerce.Location);

                // Check if the call is done correct
                if (string.IsNullOrEmpty(statusResponse.errorCode))
                {
                    // Call was maked without any problem
                    Console.WriteLine("Status: " + statusResponse.status);

                    if (statusResponse.status == "PAID")
                    {
                        var refundClient = new SwishApi.RefundClient(clientCertificate, "https://eofvqci6optquip.m.pipedream.net", "1234", "1234679304", true, SwishApi.Environment.Emulator);

                        string instructionUUID2 = Guid.NewGuid().ToString("N").ToUpper();

                        var refundResponse = refundClient.MakeRefundRequest(statusResponse.paymentReference, "0731596605", (int)statusResponse.amount, "Återköp", instructionUUID2);

                        if (string.IsNullOrEmpty(refundResponse.Error))
                        {
                            // Request OK
                            string urlForCheckingRefundStatus = refundResponse.Location;

                            // If you do a webbapplication you here should wait some time, showing a "loading" view or something and try to do the refund status check as below, you maybe have some ajax request doing a call to a actionresult doing this code
                            // Wait so that the refund has been processed
                            System.Threading.Thread.Sleep(5000);

                            // Check refund status
                            var refundCheckResposne = refundClient.CheckRefundStatus(urlForCheckingRefundStatus);

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

        // Test cancel payment
        static void MainTestCancelPayment()
        {
            var clientCertificate = new SwishApi.Models.ClientCertificate()
            {
                CertificateAsStream = System.IO.File.OpenRead("TestCert//Swish_Merchant_TestCertificate_1234679304.p12"),
                Password = "swish"
            };

            var eCommerceClient = new SwishApi.ECommerceClient(clientCertificate, "https://eow7hpmlfs99yn0.m.pipedream.net", "12345", "1234679304", true, SwishApi.Environment.Emulator);

            string instructionUUID = Guid.NewGuid().ToString("N").ToUpper();

            // Make the Payement Request
            var response = eCommerceClient.MakePaymentRequest("1234679304", 1, "Test", instructionUUID);

            // Check if the payment request got success and not got any error
            if (string.IsNullOrEmpty(response.Error))
            {
                // All OK
                string paymentReferenceURL = response.Location;

                // {"id":"63C26F360CEA403B955FCDCCE7352502","payeePaymentReference":"12345","paymentReference":"59DD805C66F44665AD44A21460B3002A","callbackUrl":"https://eow7hpmlfs99yn0.m.pipedream.net","payerAlias":"1234679304","payeeAlias":"1234679304","amount":1.00,"currency":"SEK","message":"Test","status":"CANCELLED","dateCreated":"2022-10-20T08:38:36.749Z","datePaid":null,"errorCode":"RP08","errorMessage":"The payment request has been cancelled."}
                var cancelResponse = eCommerceClient.CancelPaymentRequest(paymentReferenceURL);

                if (!string.IsNullOrEmpty(cancelResponse.status) && cancelResponse.status == "CANCELLED")
                {
                    Console.WriteLine("Payment is cancelled!");
                } else
                {
                    Console.WriteLine("Something got wrong when try to cancel the paymnent: " + cancelResponse.ErrorMessage);
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

        static void MainTestPayout()
        {
            var clientCertificate = new SwishApi.Models.ClientCertificate()
            {
                CertificateFilePath = "TestCert//Swish_Merchant_TestCertificate2_1234679304.p12",
                Password = "swish"
            };

            string sigingCertificatePath = Environment.CurrentDirectory + "\\TestCert\\Swish_Merchant_TestSigningCertificate_1234679304.p12";

            var payoutClient = new SwishApi.PayoutClient(clientCertificate, "https://eofvqci6optquip.m.pipedream.net", "1234", "1234679304", true, SwishApi.Environment.Emulator);

            string instructionUUID = Guid.NewGuid().ToString("N").ToUpper();

            // Test payeeAlias and payeeSSN from MSS_UserGuide_v1.9.pdf
            var response = payoutClient.MakePayoutRequest("46722334455", "197501088327", 1, "Test", instructionUUID, "7d70445ec8ef4d1e3a713427e973d097", new SwishApi.Models.ClientCertificate() { CertificateFilePath = sigingCertificatePath, Password = "swish" });

            if (string.IsNullOrEmpty(response.Error))
            {
                Console.WriteLine("Location: " + response.Location);

                // If you do a webbapplication you here should wait some time, showing a "loading" view or something and try to do the payment status check as below, you maybe have some ajax request doing a call to a actionresult doing this code
                // Wait so that the payment request has been processed
                System.Threading.Thread.Sleep(5000);

                // Make the payment status check
                var statusResponse = payoutClient.CheckPayoutStatus(response.Location);

                // Check if the call is done correct
                if (string.IsNullOrEmpty(statusResponse.errorCode))
                {
                    // Call was maked without any problem
                    Console.WriteLine("Status: " + statusResponse.status);

                }
                else
                {
                    // ERROR
                    Console.WriteLine("CheckPayoutResponse: " + statusResponse.errorCode + " - " + statusResponse.errorMessage);
                }
            }
            else
            {
                // ERROR
                Console.WriteLine("MakePayoutRequest - ERROR: " + response.Error);
            }

            Console.WriteLine(">>> Press enter to exit <<<");
            Console.ReadLine();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace SwishApi.Models
{
    public class RefundData
    {
        public string originalPaymentReference { get; set; }
        public string payerPaymentReference { get; set; }
        public string callbackUrl { get; set; }

        // The Swish number of the Merchant that makes the refund payment.
        public string payerAlias { get; set; }

        // The Cell phone number of the person that receives the refund payment.
        public string payeeAlias { get; set; }
        public string amount { get; set; }
        public string currency { get; set; }
        public string message { get; set; }
    }
}

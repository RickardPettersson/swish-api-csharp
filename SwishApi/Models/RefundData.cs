using System;
using System.Collections.Generic;
using System.Text;

namespace SwishApi.Models
{
    public class RefundData
    {
        public string originalPaymentReference { get; set; }
        public string callbackUrl { get; set; }
        public string payerAlias { get; set; }
        public string amount { get; set; }
        public string currency { get; set; }
        public string message { get; set; }
    }
}

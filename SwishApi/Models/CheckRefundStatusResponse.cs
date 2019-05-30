using System;
using System.Collections.Generic;
using System.Text;

namespace SwishApi.Models
{
    public class CheckRefundStatusResponse
    {
        public string errorCode { get; set; }
        public string errorMessage { get; set; }
        public string id { get; set; }
        public string payerPaymentReference { get; set; }
        public string originalPaymentReference { get; set; }
        public string callbackUrl { get; set; }
        public string payerAlias { get; set; }
        public string payeeAlias { get; set; }
        public double amount { get; set; }
        public string currency { get; set; }
        public string message { get; set; }
        public string status { get; set; }
        public DateTime dateCreated { get; set; }
        public DateTime? datePaid { get; set; }
    }
}

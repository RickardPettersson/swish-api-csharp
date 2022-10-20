using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwishApi.Models
{
    public class CancelPaymentResponse
    {
        public string id { get; set; }
        public string payeePaymentReference { get; set; }
        public string paymentReference { get; set; }
        public string callbackUrl { get; set; }
        public string payerAlias { get; set; }
        public string payeeAlias { get; set; }
        public decimal amount { get; set; }
        public string currency { get; set; }
        public string message { get; set; }
        public string status { get; set; }
        public DateTime dateCreated { get; set; }
        public DateTime? datePaid { get; set; }
        public string ErrorMessage { get; set; }
    }
}

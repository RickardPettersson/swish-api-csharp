using System;
using System.Collections.Generic;
using System.Text;

namespace SwishApi.Models
{
    public class PayoutRequestData
    {
        public string payoutInstructionUUID { get; set; }
        public string payerPaymentReference { get; set; }
        public string payerAlias { get; set; }
        public string payeeAlias { get; set; }
        public string payeeSSN { get; set; }
        public string amount { get; set; }
        public string currency { get; set; }
        public string payoutType { get; set; }
        public string message { get; set; }
        public string instructionDate { get; set; }
        public string signingCertificateSerialNumber { get; set; }
    }
}

using System;

namespace SwishApi.Models
{
    public class CheckPayoutRequestStatusResponse
    {
        // From https://developer.swish.nu/api/mss/v1#retrieve-refund-result
        // {"paymentReference":null,"payoutInstructionUUID":"AEFCF44F762E485C87DA41230861B79A" ,"payerPaymentReference":"MTS-DUMMY-PAYMENT- REF","callbackUrl":"https://myfavoritesite.dummy.domain/callback/","payerAlias":"12 34679304","payeeAlias":"46722334455","payeeSSN":"197501088327","amount":1.00,"curre ncy":"SEK","message":"Tieto Test Message","payoutType":"PAYOUT","status":"CREATED","dateCreated":"2019-12- 04T12:56:59.874","datePaid":null,"errorMessage":null,"additionalInformation":null," errorCode":null}    }
        public string paymentReference { get; set; }
        public string payoutInstructionUUID { get; set; }
        public string payerPaymentReference { get; set; }
        public string callbackUrl { get; set; }
        public string payerAlias { get; set; }
        public string payeeAlias { get; set; }
        public string payeeSSN { get; set; }
        public double amount { get; set; }
        public string currency { get; set; }
        public string message { get; set; }
        public string status { get; set; }
        public DateTime dateCreated { get; set; }
        public DateTime? datePaid { get; set; }
        public string errorMessage { get; set; }
        public string additionalInformation { get; set; }
        public string errorCode { get; set; }
    }
}

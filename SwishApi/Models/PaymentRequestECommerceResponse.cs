using System;
using System.Collections.Generic;
using System.Text;

namespace SwishApi.Models
{
    /// <summary>
    /// Response object from a Swish for Merchant Payment Request
    /// </summary>
    public class PaymentRequestECommerceResponse
    {
        public string Error { get; set; }
        public string Location { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace SwishApi.Models
{
    /// <summary>
    /// Response object from a Swish for Merchant Payment Request
    /// </summary>
    public class PaymentRequestMCommerceResponse
    {
        public string Error { get; set; }
        public string Token { get; set; }
    }
}

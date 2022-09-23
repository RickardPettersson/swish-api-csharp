using System;
using System.Collections.Generic;
using System.Text;

namespace SwishApi.Models
{
    public class PayoutRequestResponse
    {
        public string Error { get; set; }
        public string Location { get; set; }
        public string JSON { get; set; }
        public PayoutRequestData Payload { get; set; }
    }
}

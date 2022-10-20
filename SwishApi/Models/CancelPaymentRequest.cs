using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwishApi.Models
{
    public class CancelPaymentRequest
    {
        public string op { get; set; }
        public string path { get; set; }
        public string value { get; set; }
    }
}

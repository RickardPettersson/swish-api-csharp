using System;
using System.Collections.Generic;
using System.Text;

namespace SwishApi.Models
{
    public class QRCodeResponse
    {
        public string Error { get; set; }
        public byte[] Data { get; set; }
    }
}

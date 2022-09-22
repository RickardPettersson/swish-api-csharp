using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwishApi.Models
{
    public class ClientCertificate
    {
        public string CertificateFilePath { get; set; }
        public string Password { get; set; }
        public Stream CertificateAsStream { get; set; }
    }
}

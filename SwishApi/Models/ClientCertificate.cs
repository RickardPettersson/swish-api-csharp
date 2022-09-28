using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace SwishApi.Models
{
    public class ClientCertificate
    {
        public string CertificateFilePath { get; set; }
        public string Password { get; set; }
        public Stream CertificateAsStream { get; set; }
        public bool UseMachineKeySet { get; set; }
        public SecureString SecureStringPassword { get; set; }
    }
}

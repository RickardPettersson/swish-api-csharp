using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SwishApi.Models
{
    public class PayoutRequestEnvelope
    {
        public PayoutRequestData payload { get; set; }
        public string callbackUrl { get; set; }
        public string signature { get; private set; }

        public void buildSignature(string certificatePath, string certificatePassword)
        {
            using (X509Store store = new X509Store(StoreName.CertificateAuthority, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadWrite);

                var certs = new X509Certificate2Collection();
                certs.Import(certificatePath, certificatePassword, X509KeyStorageFlags.DefaultKeySet);

                RSACryptoServiceProvider csp = null;

                foreach (X509Certificate2 cert in certs)
                {
                    if (cert.HasPrivateKey)
                    {
                        csp = (RSACryptoServiceProvider)cert.PrivateKey;
                        break;
                    }
                }

                if (csp != null)
                {
                    using (SHA512 sha = new SHA512Managed())
                    {
                        var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                        var hash = sha.ComputeHash(bytes);
                        var sign = csp.SignHash(hash, CryptoConfig.MapNameToOID("SHA-512"));
                        signature = Convert.ToBase64String(sign);
                    }
                }
                else
                {
                    throw new Exception("Could not find private key");
                }
            }
        }
    }
}

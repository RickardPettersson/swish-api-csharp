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

        public void buildSignature()
        {
            X509Certificate2 cert = null;

            X509Store store = new X509Store(StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            foreach (X509Certificate2 c in store.Certificates)
            {
                if (c.SerialNumber.ToLower().Equals(payload.signingCertificateSerialNumber))
                {
                    cert = c;
                }
            }
            store.Close();

            if (cert == null)
            {
                throw new Exception("No signing certificate found!");
            }
            
            // Sign
            using (var rsa = cert.GetRSAPrivateKey()) // Get private key
            using (var sha = new SHA512Managed()) // Get SHA512 instance
            {
                var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload)); // Get payload as bytes
                var hash = sha.ComputeHash(bytes); // Hash
                // Note that hashing is done twice. Once above and once in the SignData function. This is required!
                var sign = rsa.SignData(hash, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
                signature = Convert.ToBase64String(sign); // Save signature
            }
        }
    }
}

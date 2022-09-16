using Newtonsoft.Json;
using System;
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

        private X509Certificate2 FindSignatureCertificate(string serial, string signingCertificatePath = null, string signingCertificatePassword = null)
        {
            return FindSignatureCertificateInStore(serial, StoreLocation.CurrentUser) ?? FindSignatureCertificateInStore(serial, StoreLocation.LocalMachine)
                ?? GetCertificateFromPath(signingCertificatePath, signingCertificatePassword);
        }

        private X509Certificate2 GetCertificateFromPath(string signingCertificatePath = null, string signingCertificatePassword = null)
        {
            if (signingCertificatePath == null)
            {
                return null;
            }

            return new X509Certificate2(signingCertificatePath, signingCertificatePassword);
        }

        private X509Certificate2 FindSignatureCertificateInStore(string serial, StoreLocation location)
        {
            using (var store = new X509Store(location))
            {
                store.Open(OpenFlags.ReadOnly);
                foreach (X509Certificate2 c in store.Certificates)
                {
                    if (c.SerialNumber.ToLower().Equals(serial.ToLower()))
                    {
                        return c;
                    }
                }
            }
            return null;
        }

        public void buildSignature(string signingCertificatePath = null, string signingCertificatePassword = null)
        {
            X509Certificate2 cert = FindSignatureCertificate(payload.signingCertificateSerialNumber, signingCertificatePath, signingCertificatePassword);
            
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

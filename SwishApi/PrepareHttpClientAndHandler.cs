using SwishApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SwishApi
{
    public class PrepareHttpClientAndHandler
    {
        public static void LoadCert(ClientCertificate certificate, out HttpClientHandler handler, out HttpClient client, bool enableHTTPLog = false)
        {
            handler = new HttpClientHandler();

            if (certificate != null)
            {
                if (certificate.UseMachineKeySet)
                {
                    if (string.IsNullOrEmpty(certificate.Password))
                    {
                        if (certificate.SecureStringPassword != null)
                        {
                            if (certificate.CertificateAsStream != null)
                            {
                                var cert = new X509Certificate2(Misc.ReadFully(certificate.CertificateAsStream),
                                    certificate.SecureStringPassword, X509KeyStorageFlags.MachineKeySet);

                                handler.ClientCertificates.Add(cert);
                            }
                            else
                            {
                                var cert = new X509Certificate2(certificate.CertificateFilePath,
                                    certificate.SecureStringPassword, X509KeyStorageFlags.MachineKeySet);

                                handler.ClientCertificates.Add(cert);
                            }
                        }
                        else
                        {
                            throw new Exception(
                                "Certificate password missing set wish needed to use with MachineKeySet");
                        }
                    }
                    else
                    {
                        if (certificate.CertificateAsStream != null)
                        {
                            var cert = new X509Certificate2(Misc.ReadFully(certificate.CertificateAsStream),
                                certificate.Password, X509KeyStorageFlags.MachineKeySet);

                            handler.ClientCertificates.Add(cert);
                        }
                        else
                        {
                            var cert = new X509Certificate2(certificate.CertificateFilePath,
                                certificate.Password, X509KeyStorageFlags.MachineKeySet);

                            handler.ClientCertificates.Add(cert);
                        }
                    }
                }
                else
                {
                    var certBytes = string.IsNullOrEmpty(certificate.CertificateFilePath) ?
                        Misc.ReadFully(certificate.CertificateAsStream) :
                        File.ReadAllBytes(certificate.CertificateFilePath);

                    SetCertificate(handler, certBytes, certificate.Password);

                }
            }

            client = new HttpClient(new LoggingHandler(handler, enableHTTPLog));
        }

        private static void SetCertificate(HttpClientHandler handler, byte[] certBytes, string password)
        {
            var certs = new X509Certificate2Collection();

            certs.Import(certBytes, password);

            foreach (var cert in certs)
            {
                if (cert.HasPrivateKey)
                {
                    handler.ClientCertificates.Add(cert);
                }
                else
                {
                    //Add the intermediate certificate to the trusted root store
                    //which acts as a cache during the TLS handshake
                    using var store = new X509Store(StoreName.CertificateAuthority,
                        StoreLocation.CurrentUser);
                    store.Open(OpenFlags.ReadWrite);
                    store.Add(cert);
                }
            }
        }
    }
}

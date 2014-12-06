// Secure Sockets Layer / Transport Security Layer Implementation
// Copyright(c) 2004-2005 Carlos Guzman Alvarez

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files(the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Globalization;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace SecureSocketLayer.Net.Security.Providers.Common
{
    internal sealed class X509ChainValidator : IX509ChainValidator
    {
        #region · Fields ·

        private X509Chain               certificateChain;
        private ExchangeAlgorithmType   exchangeAlgorithmType;

        #endregion

        #region · Constructors ·

        public X509ChainValidator(X509Chain certificateChain, ExchangeAlgorithmType exchangeAlgorithmType)
        {
            this.certificateChain       = certificateChain;
            this.exchangeAlgorithmType  = exchangeAlgorithmType;
        }

        #endregion

        #region · IX509ChainValidator Methods ·

        public SslPolicyErrors Validate(X509CertificateCollection certificates, string targetHost)
        {
            SslPolicyErrors errors = SslPolicyErrors.None;

            // the leaf is the web server certificate
            X509Certificate2 leaf = new X509Certificate2(certificates[0].GetRawCertData());

            // SSL specific check - not all certificates can be 
            // used to server-side SSL some rules applies after 
            // all ;-)
            if (!this.CheckCertificateUsage(leaf)) 
            {
                // WinError.h CERT_E_PURPOSE 0x800B0106
                errors |= SslPolicyErrors.RemoteCertificateChainErrors;
            }

            // SSL specific check - does the certificate match 
            // the host ?
            if (!this.CheckServerIdentity(leaf, targetHost))
            {
                // WinError.h CERT_E_CN_NO_MATCH 0x800B010F
                errors |= SslPolicyErrors.RemoteCertificateNameMismatch;
            }

            return errors;
        }

        #endregion

        #region · Private Methods ·

        // Note: this method only works for RSA certificates
        // DH certificates requires some changes - does anyone use one ?
        private bool CheckCertificateUsage(X509Certificate2 cert) 
        {
            // certificate extensions are required for this
            // we "must" accept older certificates without proofs
            if (cert.Version < 3)
            {
                return true;
            }

            X509KeyUsageFlags ku = X509KeyUsageFlags.None;

            switch (this.exchangeAlgorithmType) 
            {
                case ExchangeAlgorithmType.RsaSign:
                    ku = X509KeyUsageFlags.DigitalSignature;
                    break;
                
                case ExchangeAlgorithmType.RsaKeyX:
                    ku = X509KeyUsageFlags.KeyEncipherment;
                    break;
                
                case ExchangeAlgorithmType.DiffieHellman:
                    ku = X509KeyUsageFlags.KeyAgreement;
                    break;
            }

            X509KeyUsageExtension           kux = null;
            X509EnhancedKeyUsageExtension   eku = null;

            X509Extension xtn = cert.Extensions["2.5.29.15"];
            if (xtn != null)
            {
                kux = new X509KeyUsageExtension(xtn, xtn.Critical);
            }            

            xtn = cert.Extensions["2.5.29.37"];
            if (xtn != null)
            {
                eku = new X509EnhancedKeyUsageExtension(xtn, xtn.Critical);
            }

            if (kux != null && eku != null) 
            {
                // RFC3280 states that when both KeyUsageExtension and 
                // ExtendedKeyUsageExtension are present then BOTH should
                // be valid
                return ((kux.KeyUsages & ku) == ku && this.CheckEnhacedKeyUsage(eku, "1.3.6.1.5.5.7.3.1"));
            }
            else if (kux != null) 
            {
                return ((kux.KeyUsages & ku) == ku);
            }
            else if (eku != null) 
            {
                // Server Authentication(1.3.6.1.5.5.7.3.1)
                // return eku.EnhancedKeyUsages.Contains("1.3.6.1.5.5.7.3.1");
                return this.CheckEnhacedKeyUsage(eku, "1.3.6.1.5.5.7.3.1");
            }

            // last chance - try with older(deprecated) Netscape extensions
            xtn = cert.Extensions ["2.16.840.1.113730.1.1"];
            if (xtn != null) 
            {
#warning Change this
                /*
                NetscapeCertTypeExtension ct = new NetscapeCertTypeExtension(xtn);
                return ct.Support(NetscapeCertTypeExtension.CertTypes.SslServer);
                */
            }

            // certificate isn't valid for SSL server usage
            return false;
        }

        private bool CheckEnhacedKeyUsage(X509EnhancedKeyUsageExtension extension, string oid)
        {
            foreach (Oid keyUsage in extension.EnhancedKeyUsages)
            {
                if (keyUsage.FriendlyName == oid)
                {
                    return true;
                }
            }

            return false;
        }

        // RFC2818 - HTTP Over TLS, Section 3.1
        // http://www.ietf.org/rfc/rfc2818.txt
        // 
        // 1.	if present MUST use subjectAltName dNSName as identity
        // 1.1.		if multiples entries a match of any one is acceptable
        // 1.2.		wildcard * is acceptable
        // 2.	URI may be an IP address -> subjectAltName.iPAddress
        // 2.1.		exact match is required
        // 3.	Use of the most specific Common Name(CN=) in the Subject
        // 3.1		Existing practice but DEPRECATED
        private bool CheckServerIdentity(X509Certificate cert, string targetHost) 
        {
            X509Extension ext = ((X509Certificate2)cert).Extensions["2.5.29.17"];

            // 1. subjectAltName
            if (ext != null) 
            {
                X500DistinguishedName subjectName = new X500DistinguishedName(ext.RawData);
                X509SubjectKeyIdentifierExtension ski = new X509SubjectKeyIdentifierExtension(ext, ext.Critical);

#warning Fix this
                /*
                SubjectAltNameExtension subjectAltName = new SubjectAltNameExtension();

                // 1.1 - multiple DNSName
                foreach (string dns in subjectName.DNSNames) 
                {
                    // 1.2 TODO - wildcard support
                    if (dns == targetHost)
                    {
                        return true;
                    }
                }

                // 2. ipAddress
                foreach (string ip in subjectAltName.IPAddresses) 
                {
                    // 2.1. Exact match required
                    if (ip == targetHost)
                    {
                        return true;
                    }
                }
                */
            }

            // 3. Common Name(CN=)
            return this.CheckDomainName(cert.Subject, targetHost);
        }

        private bool CheckDomainName(string subjectName, string targetHost)
        {
            string	domainName = String.Empty;
            Regex search = new Regex(@"CN\s*=\s*([^,]*)");

            MatchCollection	elements = search.Matches(subjectName);

            if (elements.Count == 1)
            {
                if (elements[0].Success)
                {
                    domainName = elements[0].Groups[1].Value.ToString();
                }
            }

            // TODO: add wildcard * support
            return(String.Compare(targetHost, domainName, true, CultureInfo.InvariantCulture) == 0);

            /*
             * the only document found describing this is:
             * http://www.geocities.com/SiliconValley/Byte/4170/articulos/tls/autentic.htm#Autenticaci%F3n%20del%20Server
             * however I don't see how this could deal with wildcards ?
             * other issues
             * a. there could also be many address returned
             * b. Address property is obsoleted in .NET 1.1
             * 
                        if (domainName == String.Empty)
                        {
                            return false;
                        }
                        else
                        {
                            string targetHost = context.ClientSettings.TargetHost;

                            // Check that the IP is correct
                            try
                            {
                                IPAddress	ipHost		= Dns.Resolve(targetHost).AddressList[0];
                                IPAddress	ipDomain	= Dns.Resolve(domainName).AddressList[0];

                                // Note: Address is obsolete in 1.1
                                return(ipHost.Address == ipDomain.Address);
                            }
                            catch (Exception)
                            {
                                return false;
                            }
                        }*/
        }

        #endregion
    }
}

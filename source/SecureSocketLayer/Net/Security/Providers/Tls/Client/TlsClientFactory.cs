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

#if NET_2_0

using System;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using SecureSocketLayer.Net.Security.Providers.Common;
using SecureSocketLayer.Net.Security.Providers.Common.Client;

namespace SecureSocketLayer.Net.Security.Providers.Tls.Client
{
    internal sealed class TlsClientFactory : ISecureFactory
    {
        #region · Static Properties ·

        public static readonly ISecureFactory Instance = new TlsClientFactory();

        #endregion

        #region · Properties ·

#warning FIXME !!! Both properties needs to be properly implemented

        public CipherSuiteCollection SupportedCipherSuites
        {
            get { return CipherSuiteFactory.GetSupportedCiphers(SslProtocols.Tls); }
        }

        public CompressionMethodCollection SupportedCompressionMethods
        {
            get 
            {
                CompressionMethodCollection c = new CompressionMethodCollection();
                c.Add(new NullCompressionMethod());

                return c;
            } 
        }

        #endregion

        #region · Private Constructors ·

        private TlsClientFactory()
        {
        }

        #endregion

        #region · Methods ·

        public ISecureAuthenticator CreateSecureAuthenticator(SecureSession session)
        {
            ISecureAuthenticator auth = new ClientSecureAuthenticator(session);
            auth.MessageGenerator = CreateHandshakeMessageGenerator(auth);
            auth.MessageProcessor = CreateHandshakeMessageProcessor(auth);

            return auth;
        }

        public IHandshakeMessageGenerator CreateHandshakeMessageGenerator(ISecureAuthenticator authenticator)
        {
            return new TlsHandshakeMessageGenerator(authenticator);
        }
        
        public IHandshakeMessageProcessor CreateHandshakeMessageProcessor(ISecureAuthenticator authenticator)
        {
            return new TlsHandshakeMessageProcessor(authenticator);
        }

        public ISecureProtocol CreateSecureProtocol(SecureSession session)
        {
            return new SecureClientProtocol(session);
        }

        public ISecureKeyGenerator CreateKeyGenerator(SecureSession session)
        {
            return new TlsSecureKeyGenerator(session);
        }

        public ISecureKeyInfo CreateKeyInfo()
        {
            return new SecureKeyInfo();
        }

        public IRecordEncryptor	CreateRecordEncryptor(SecureSession session)
        {
            return new ClientRecordEncryptor(session.CipherSuite, session.SecureKeyInfo);
        }

        public IRecordDecryptor	CreateRecordDecryptor(SecureSession session)
        {
            return new ClientRecordDecryptor(session.CipherSuite, session.SecureKeyInfo);
        }

        public IRecordMACManager CreateMACManager(SecureSession session)
        {
            return new TlsClientRecordMACManager(session);
        }

        public IX509ChainValidator CreateX509ChainValidator(X509Chain cerficateChain, ExchangeAlgorithmType exchangeAlgorithmType)
        {
            return new X509ChainValidator(cerficateChain, exchangeAlgorithmType);
        }

        #endregion
    }
}

#endif
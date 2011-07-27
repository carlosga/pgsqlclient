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
using System.Security.Cryptography.X509Certificates;

namespace SecureSocketLayer.Net.Security.Providers.Common.Server
{
    internal abstract class ServerHandshakeMessageGenerator : IHandshakeMessageGenerator
    {
        #region · Fields ·

        private ISecureAuthenticator authenticator;

        #endregion

        #region · Protected Properties ·

        protected ISecureAuthenticator Authenticator
        {
            get { return this.authenticator; }
        }

        #endregion

        #region · Protected Constructors ·

        protected ServerHandshakeMessageGenerator(ISecureAuthenticator authenticator)
        {
            this.authenticator = authenticator;
        }

        #endregion

        #region · IHandshakeMessageGenerator Members ·

        public virtual byte[] ClientHello()
        {
            return null;
        }

        public virtual byte[] Certificate()
        {
            return null;
        }

        public virtual byte[] ServerKeyExchange()
        {
            return null;
        }

        public virtual byte[] CertificateRequest()
        {
            return null;
        }

        public virtual byte[] ServerHello()
        {
            return null;
        }

        public virtual byte[] ServerHelloDone()
        {
            return null;
        }

        public virtual byte[] CertificateVerify()
        {
            return null;
        }

        public virtual byte[] ClientKeyExchange()
        {
            return null;
        }

        public virtual byte[] Finished()
        {
            return null;
        }

        #endregion

        #region · Protected Methods ·

        protected virtual void WriteMessageLength(MemoryStreamEx message)
        {
            message.Position = 1;
            message.WriteInt24(message.Length - 4);
        }

        #endregion
    }
}

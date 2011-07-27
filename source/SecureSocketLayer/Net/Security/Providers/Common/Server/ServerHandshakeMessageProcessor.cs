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
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using SecureSocketLayer.Net.Security.Providers.Cryptography;
using Mono.Security;

namespace SecureSocketLayer.Net.Security.Providers.Common.Server
{
	internal abstract class ServerHandshakeMessageProcessor : IHandshakeMessageProcessor
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

        protected ServerHandshakeMessageProcessor(ISecureAuthenticator authenticator)
		{
			this.authenticator = authenticator;
		}

		#endregion

		#region · IHandshakeMessageProcessor Members ·

		public virtual void ClientHello(byte[] buffer)
		{
		}

		public virtual void ServerHello(byte[] buffer)
		{
		}

        public virtual void Certificate(byte[] buffer)
        {
        }

		public virtual void ServerKeyExchange(byte[] buffer)
		{
		}

		public virtual void CertificateRequest(byte[] buffer)
		{
		}

		public virtual void ServerHelloDone(byte[] buffer)
		{
		}

		public virtual void CertificateVerify(byte[] buffer)
		{
		}

		public virtual void ClientKeyExchange(byte[] buffer)
		{
		}

		public virtual void Finished(byte[] buffer)
		{
		}

		#endregion
	}
}

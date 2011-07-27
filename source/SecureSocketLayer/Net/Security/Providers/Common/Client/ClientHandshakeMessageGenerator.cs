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

namespace SecureSocketLayer.Net.Security.Providers.Common.Client
{
	internal abstract class ClientHandshakeMessageGenerator : IHandshakeMessageGenerator
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

		protected ClientHandshakeMessageGenerator(ISecureAuthenticator authenticator)
		{
			this.authenticator = authenticator;
		}

		#endregion

		#region · IHandshakeMessageGenerator Members ·

		public virtual byte[] ClientHello()
		{
			SecureSession	session = authenticator.SecureSession;
			MemoryStreamEx	message	= new MemoryStreamEx();

			// Message Type
			message.Write((byte)HandshakeMessageType.ClientHello);

			// Message Length
			message.WriteInt24(0);

			// Client Version
			message.Write(Helper.GetProtocolCode(session.CurrentProtocolType));
								
			// Random bytes - Unix time + Radom bytes [28]
			byte[] random = Helper.GenerateRandom();
			message.Write(random);
			if (this.authenticator.GeneratedClientRandom != null)
			{
				this.authenticator.GeneratedClientRandom(random);
			}

			// Send the empty session ID
			message.Write((byte)0);
			
			// Write Supported Cipher Suites
			message.Write((short)(session.SupportedCipherSuites.Count * 2));

			for (int i = 0; i < session.SupportedCipherSuites.Count; i++)
			{
				message.Write((short)session.SupportedCipherSuites[i].Code);
			}

			// Write supported Compression Methods
			message.Write((byte)session.SupportedCompressionMethods.Count);

			for (int i = 0; i < session.SupportedCompressionMethods.Count; i++)
			{
				message.Write((byte)session.SupportedCompressionMethods[i].Code);
			}

			// Write length
			this.WriteMessageLength(message);
		
			return message.ToArray();
		}

		public virtual byte[] Certificate()
		{
#warning "Client certificate selection is unfinished"
			SecureSession	session = this.authenticator.SecureSession;
			string			msg		= "Client certificate requested by the server and no client certificate specified.";

			if (session.LocalCertificates.Count == 0)
			{
				// AlertDescription.UserCancelled
				throw new SecureException(msg);
			}
			
			// Select a valid certificate
			X509Certificate clientCert = session.LocalCertificates[0];

			// Update the selected client certificate
			if (this.authenticator.ClientCertificateSelected != null)
			{
				this.authenticator.ClientCertificateSelected(clientCert);
			}

			// Write client certificates information to a stream
			MemoryStreamEx stream = new MemoryStreamEx();

			stream.WriteInt24(0);
			stream.WriteInt24(session.ClientCertificate.GetRawCertData().Length);
			stream.Write(session.ClientCertificate.GetRawCertData());

			stream.Position = 0;
			stream.WriteInt24((int)stream.Length - 3);

			this.WriteMessageLength(stream);

			return stream.ToArray();
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

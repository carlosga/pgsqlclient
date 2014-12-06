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

using SecureSocketLayer.Net.Security.Providers.Common;
using SecureSocketLayer.Net.Security.Providers.Common.Client;
using System.Security.Cryptography;

namespace SecureSocketLayer.Net.Security.Providers.Ssl.Client
{
	internal sealed class SslHandshakeMessageGenerator: ClientHandshakeMessageGenerator
	{
		#region · Constructors ·

		public SslHandshakeMessageGenerator(ISecureAuthenticator authenticator)
			: base(authenticator)
		{
		}

		#endregion

		#region · Methods ·

		public override byte[] ClientKeyExchange()
		{
			SecureSession	session = this.Authenticator.SecureSession;
			MemoryStreamEx	message	= new MemoryStreamEx();

			if (this.Authenticator.ExchangingKeys != null)
			{
				this.Authenticator.ExchangingKeys();
			}

			// Message Type
			message.Write((byte)HandshakeMessageType.ClientKeyExchange);

			// Message Length
			message.WriteInt24(0);

			// Get the RSA key
			RSA rsa = session.GetRSAExchangeAlgortihm();
			
			// Encrypt premaster_sercret
			RSAPKCS1KeyExchangeFormatter formatter = new RSAPKCS1KeyExchangeFormatter(rsa);

			// Write the preMasterSecret encrypted
			message.Write(formatter.CreateKeyExchange(session.SecureKeyInfo.PreMasterSecret));

			// Write length
			this.WriteMessageLength(message);

			if (this.Authenticator.ExchangedKeys != null)
			{
				this.Authenticator.ExchangedKeys();
			}
			
			return message.ToArray();
		}

		public override byte[] Finished()
		{
			SecureSession	session = this.Authenticator.SecureSession;
			MemoryStreamEx	message	= new MemoryStreamEx();

			// Message Type
			message.Write((byte)HandshakeMessageType.Finished);

			// Message Length
			message.WriteInt24(0);

			// Compute handshake messages hashes
			HashAlgorithm hash = new HandshakeMessageHash(session.SecureKeyInfo.MasterSecret);

			MemoryStreamEx data = new MemoryStreamEx();
			data.Write(session.HandshakeMessages);
			data.Write((int)0x434C4E54);
			
			hash.TransformFinalBlock(data.ToArray(), 0,(int)data.Length);

			message.Write(hash.Hash);

			this.WriteMessageLength(message);

			return message.ToArray();
		}

		public override byte[] CertificateVerify()
		{
			MemoryStreamEx	message	= new MemoryStreamEx();
			SecureSession	session	= this.Authenticator.SecureSession;

			// Message Type
			message.Write((byte)HandshakeMessageType.CertificateVerify);

			// Message Length
			message.WriteInt24(0);

			HandshakeMessageHash hash = new HandshakeMessageHash(session.SecureKeyInfo.MasterSecret);			
			hash.TransformFinalBlock(
				session.HandshakeMessages,
				0, 
				session.HandshakeMessages.Length);

			// RSAManaged of the selected ClientCertificate 
			// (at this moment the first one)
#warning Get the Client Certificate Private key
			RSA rsa = null;
				// this.getClientCertRSA((RSA)privKey);

			// Write message
			byte[] signature = hash.CreateSignature(rsa);
			message.Write((short)signature.Length);
			message.Write(signature, 0, signature.Length);

			this.WriteMessageLength(message);

			return message.ToArray();
		}

		#endregion
	}
}

#endif
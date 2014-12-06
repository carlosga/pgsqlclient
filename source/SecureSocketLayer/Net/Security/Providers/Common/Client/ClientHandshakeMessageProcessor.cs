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

using Mono.Security;
using SecureSocketLayer.Net.Security.Providers.Cryptography;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SecureSocketLayer.Net.Security.Providers.Common.Client
{
	internal abstract class ClientHandshakeMessageProcessor : IHandshakeMessageProcessor
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

		protected ClientHandshakeMessageProcessor(ISecureAuthenticator authenticator)
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
			MemoryStreamEx message = new MemoryStreamEx(buffer);

			// Read protocol version
			short protocol = message.ReadInt16();
			if (protocol != Helper.GetProtocolCode(authenticator.SecureSession.CurrentProtocolType))
			{
				if (this.authenticator.ProtocolChanged != null)
				{
					this.authenticator.ProtocolChanged(protocol);
				}
			}
			
			// Read random  - Unix time + Random bytes
			byte[] random = message.ReadBytes(32);
			if (this.authenticator.ReceivedServerRandom != null)
			{
				this.authenticator.ReceivedServerRandom(random);
			}
						
			// Read Session id
			int length = (int)message.ReadByte();
			if (length > 0)
			{
				message.ReadBytes(length);
			}

			// Read cipher suite
			short cipherCode = message.ReadInt16();
			if (this.authenticator.CipherSuiteSelected != null)
			{
				this.authenticator.CipherSuiteSelected(cipherCode);
			}
			
			// Read compression methods( always 0 )
			byte compressionMethod = message.ReadByte();
			if (this.authenticator.CompressionMethodSelected != null)
			{
				this.authenticator.CompressionMethodSelected(compressionMethod);
			}
		}

		public virtual void Certificate(byte[] buffer)
		{
			MemoryStreamEx message = new MemoryStreamEx(buffer);
		
			int readed	= 0;
			int length	= message.ReadInt24();

			while (readed < length)
			{
				// Read certificate length
				int certLength = message.ReadInt24();

				// Increment readed
				readed += 3;

				if (certLength > 0)
				{
					// Read certificate data
					byte[] rawCert = message.ReadBytes(certLength);

					if (this.authenticator.RemoteCertificateReceived != null)
					{
						this.authenticator.RemoteCertificateReceived(new X509Certificate(rawCert));
					}

					readed += certLength;
				}
			}

			if (this.authenticator.ReceivedRemoteCertificateChain != null)
			{
				this.authenticator.ReceivedRemoteCertificateChain();
			}
		}

		public virtual void ServerKeyExchange(byte[] buffer)
		{
			MemoryStreamEx	message		= new MemoryStreamEx(buffer);
			SecureSession	session		= this.authenticator.SecureSession;
			RSAParameters	rsaParams	= new RSAParameters();
			
			// Read modulus
			rsaParams.Modulus	= message.ReadBytes(message.ReadInt16());

			// Read exponent
			rsaParams.Exponent	= message.ReadBytes(message.ReadInt16());

			// Read signed params
			byte[] signedParams	= message.ReadBytes(message.ReadInt16());

			// Verifiy signed params
			MD5SHA1 hash = new MD5SHA1();

			// Calculate size of server params
			int size = rsaParams.Modulus.Length + rsaParams.Exponent.Length + 4;

			// Create server params array
			MemoryStreamEx stream = new MemoryStreamEx();

			stream.Write(session.SecureKeyInfo.ClientServerRandom);
			stream.Write(message.ToArray(), 0, size);

			hash.ComputeHash(stream.ToArray());

			stream.Reset();
			
			if (hash.VerifySignature(session.GetServerCertificateRSA(), signedParams))
			{
				// AlertDescription.DecodeError,
				throw new SecureException("Data was not signed with the server certificate.");
			}

			if (this.authenticator.ReceivedServerKeyExchange != null)
			{
				this.authenticator.ReceivedServerKeyExchange(rsaParams);
			}
		}

		public virtual void CertificateRequest(byte[] buffer)
		{
			MemoryStreamEx	message		= new MemoryStreamEx(buffer);
			SecureSession	session		= this.authenticator.SecureSession;

			// Read requested certificate types
			int typesCount = message.ReadByte();
						
			ClientCertificateType[] certificateTypes = new ClientCertificateType[typesCount];

			for (int i = 0; i < typesCount; i++)
			{
				certificateTypes[i] = (ClientCertificateType)message.ReadByte();
			}

			/*
			 * Read requested certificate authorities(Distinguised Names)
			 * 
			 * Name ::= SEQUENCE OF RelativeDistinguishedName
			 * 
			 * RelativeDistinguishedName ::= SET OF AttributeValueAssertion
			 * 
			 * AttributeValueAssertion ::= SEQUENCE {
			 * attributeType OBJECT IDENTIFIER
			 * attributeValue ANY }
			 */

			string[] distinguisedNames = null;

			if (message.ReadInt16() != 0)
			{
				ASN1 rdn = new ASN1(message.ReadBytes(message.ReadInt16()));

				distinguisedNames = new string[rdn.Count];

				for (int i = 0; i < rdn.Count; i++)
				{
					// element[0] = attributeType
					// element[1] = attributeValue
					ASN1 element = new ASN1(rdn[i].Value);

					distinguisedNames[i] = Encoding.UTF8.GetString(element[1].Value);
				}
			}

			if (this.authenticator.ClientCertificateRequested != null)
			{
				this.authenticator.ClientCertificateRequested(certificateTypes, distinguisedNames);
			}
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

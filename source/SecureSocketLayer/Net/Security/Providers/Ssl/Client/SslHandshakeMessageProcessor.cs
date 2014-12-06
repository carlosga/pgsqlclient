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
	internal sealed class SslHandshakeMessageProcessor : ClientHandshakeMessageProcessor
	{
		#region · Constructors ·

		public SslHandshakeMessageProcessor(ISecureAuthenticator authenticator)
			: base(authenticator)
		{
		}

		#endregion

		#region · Methods ·

		public override void Finished(byte[] buffer)
		{
			SecureSession	session = this.Authenticator.SecureSession;
			MemoryStreamEx	message = new MemoryStreamEx(buffer);

			// Compute handshake messages hashes
			HashAlgorithm hash = new HandshakeMessageHash(session.SecureKeyInfo.MasterSecret);

			MemoryStreamEx data = new MemoryStreamEx();
			data.Write(session.HandshakeMessages);
			data.Write((int)0x53525652);
			
			hash.TransformFinalBlock(data.ToArray(), 0,(int)data.Length);

			data.Reset();

			byte[] serverHash	= message.ReadBytes((int)buffer.Length);			
			byte[] clientHash	= hash.Hash;
			
			// Check server prf against client prf
			if (!Helper.Compare(clientHash, serverHash))
			{
				// AlertDescription.InsuficientSecurity
				throw new SecureException("Invalid ServerFinished message received.");
			}			
		}

		#endregion
	}
}

#endif
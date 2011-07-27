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
using System.IO;
using System.Security.Authentication;

namespace SecureSocketLayer.Net.Security.Providers.Common
{
	internal class SecureKeyGenerator : ISecureKeyGenerator
	{
		#region · Protected Static Members ·

		protected static readonly byte[] EmptyArray = new byte[0];

		#endregion

		#region · Fields ·

		private SecureSession session;

		#endregion

		#region · Protected Properties ·

		protected SecureSession Session
		{
			get { return this.session; }
		}

		#endregion

		#region · Protected Constructors ·

		protected SecureKeyGenerator(SecureSession session)
		{
			this.session = session;
		}

		#endregion

		#region · ISecureKeyGenerator Members ·

		public virtual void Generate(ref ISecureKeyInfo keyInfo)
		{
			throw new NotSupportedException();
		}

		#endregion

		#region · Protected Methods ·

		protected virtual byte[] CreatePremasterSecret()
		{
			MemoryStreamEx stream = new MemoryStreamEx();
			
			// Write protocol version
			// We need to send here the protocol version used in 
			// the ClientHello message, that can be different than the actual
			// protocol version
			stream.Write(this.Session.GetClientHelloProtocol());

			// Generate random bytes
			stream.Write(Helper.GetSecureRandomBytes(46));

			return stream.ToArray();
		}

		#endregion
	}
}

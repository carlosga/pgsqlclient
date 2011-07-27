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
using System.Security.Cryptography;

namespace SecureSocketLayer.Net.Security.Providers.Common.Client
{
	internal sealed class SecureClientProtocol : SecureProtocol
	{
		#region · Constructors ·

		public SecureClientProtocol(SecureSession session) :
			base(session)
		{
		}

		#endregion

		#region · Protected Methods ·

		protected override byte[] Encrypt(SecureRecord record)
		{
			IRecordMACManager	macManager	= this.Session.MACManager;
			IRecordEncryptor	encryptor	= this.Session.Encryptor;
			byte[]				mac			= macManager.CreateClientRecordMAC(record);

			// Encrypt the message
			byte[] ecr = encryptor.Encrypt(record.Fragment, mac);

			// Set new Client Cipher IV
			if (this.Session.CipherSuite.CipherMode == CipherMode.CBC)
			{
				byte[] iv = new byte[this.Session.CipherSuite.IvSize];
				Buffer.BlockCopy(ecr, ecr.Length - iv.Length, iv, 0, iv.Length);

				encryptor.UpdateClientCipherIV(iv);
			}

			// The call to the base class should update sequence number
			base.Encrypt(record);

			return ecr;
		}

		protected override byte[] Decrypt(SecureRecord record)
		{
			IRecordMACManager	macManager	= this.Session.MACManager;
			IRecordDecryptor	decryptor	= this.Session.Decryptor;
			byte[]				dcrFragment	= null;
			byte[]				dcrMAC		= null;

			try
			{
				decryptor.Decrypt(record.Fragment, ref dcrFragment, ref dcrMAC);
			}
			catch
			{
				throw;
			}
			
			// Generate new record and validate the MAC
			record.Fragment	= dcrFragment;

			if (!macManager.ValidateServerRecordMAC(record, dcrMAC))
			{
				throw new SecureException("Bad record MAC");
			}

			// The call to the base class should update the sequence number
			base.Decrypt(record);

			return dcrFragment;
		}

		#endregion
	}
}

#endif
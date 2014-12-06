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
using System.Security.Authentication;
using System.Security.Cryptography;

namespace SecureSocketLayer.Net.Security.Providers.Ssl
{
	internal class SslRecordMACManager : IRecordMACManager
	{
		#region · Fields ·

		private byte[]			pad1;
		private byte[]			pad2;
		private SecureSession	session; 
		private HashAlgorithm	hashAlgorithm;

		#endregion

		#region · Protected Properties ·

		protected SecureSession Session
		{
			get { return this.session; }
		}

		protected HashAlgorithm HashAlgorithm
		{
			get { return this.hashAlgorithm; }
		}

		#endregion

		#region · Protected Constructors ·

		protected SslRecordMACManager()
		{
		}

		protected SslRecordMACManager(SecureSession session)
		{
			this.SetSecureSession(session);
		}

		#endregion

		#region · Methods ·

		public virtual void SetSecureSession(SecureSession session)
		{
			this.session		= session;
			this.hashAlgorithm	= HashAlgorithm.Create(session.CipherSuite.HashAlgorithmName);
			
			int padLength = (this.session.CipherSuite.HashAlgorithmType == HashAlgorithmType.Md5) ? 48 : 40;

			// Fill pad arrays
			this.pad1 = new byte[padLength];
			this.pad2 = new byte[padLength];

			/* Pad the key for inner and outer digest */
			for (int i = 0; i < padLength; ++i) 
			{
				this.pad1[i] = 0x36;
				this.pad2[i] = 0x5C;
			}
		}

		public virtual byte[] CreateClientRecordMAC(SecureRecord record)
		{
			return null;
		}

		public virtual byte[] CreateServerRecordMAC(SecureRecord record)
		{
			return null;
		}

		public virtual bool ValidateClientRecordMAC(SecureRecord record, byte[] mac)
		{
			return Helper.Compare(mac, this.CreateClientRecordMAC(record));
		}

		public virtual bool ValidateServerRecordMAC(SecureRecord record, byte[] mac)
		{
			return Helper.Compare(mac, this.CreateServerRecordMAC(record));
		}

		#endregion

		#region · Protected Methods ·

		protected byte[] CreateRecordMAC(SecureRecord record, byte[] key, long sequence)
		{
			MemoryStreamEx block = new MemoryStreamEx();

			block.Write(key);
			block.Write(this.pad1);
			block.Write(sequence);
			block.Write((byte)record.ContentType);
			block.Write((short)record.Fragment.Length);
			block.Write(record.Fragment);
			
			this.HashAlgorithm.ComputeHash(block.ToArray(), 0,(int)block.Length);

			byte[] blockHash = this.HashAlgorithm.Hash;

			block.Reset();

			block.Write(key);
			block.Write(this.pad2);
			block.Write(blockHash);

			this.HashAlgorithm.ComputeHash(block.ToArray(), 0,(int)block.Length);

			block.Reset();

			return this.HashAlgorithm.Hash;
		}

		#endregion
	}
}

#endif
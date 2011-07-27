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
using System.Security.Authentication;
using System.Security.Cryptography;
using SecureSocketLayer.Net.Security.Providers.Common;
using SecureSocketLayer.Net.Security.Providers.Cryptography;

namespace SecureSocketLayer.Net.Security.Providers.Tls
{
	internal class TlsRecordMACManager : IRecordMACManager
	{
		#region · Fields ·

		private SecureSession		session; 
		private KeyedHashAlgorithm	clientHMAC;
		private KeyedHashAlgorithm	serverHMAC;

		#endregion

		#region · Protected Properties ·

		protected SecureSession Session
		{
			get { return this.session; }
		}

		protected KeyedHashAlgorithm ClientHMAC
		{
			get { return this.clientHMAC; }
		}
		
		protected KeyedHashAlgorithm ServerHMAC
		{
			get { return this.serverHMAC; }
		}

		#endregion

		#region · Protected Constructors ·

		protected TlsRecordMACManager()
		{
		}

		protected TlsRecordMACManager(SecureSession session)
		{
			this.SetSecureSession(session);
		}

		#endregion

		#region · Methods ·

		public virtual void SetSecureSession(SecureSession session)
		{
			this.session	= session;
			this.clientHMAC = new HMACSsl(
				this.Session.CipherSuite.HashAlgorithmName,
				this.session.SecureKeyInfo.ClientWriteMAC);

			this.serverHMAC = new HMACSsl(
				this.Session.CipherSuite.HashAlgorithmName,
				this.session.SecureKeyInfo.ServerWriteMAC);
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

		protected byte[] CreateRecordMAC(SecureRecord record, KeyedHashAlgorithm algorithm, long sequence)
		{
			MemoryStreamEx data = new MemoryStreamEx();

			data.Write(sequence);
			data.Write((byte)record.ContentType);
			data.Write(record.Protocol);
			data.Write((short)record.Fragment.Length);
			data.Write(record.Fragment);
			
			return algorithm.ComputeHash(data.ToArray());
		}

		#endregion
	}
}

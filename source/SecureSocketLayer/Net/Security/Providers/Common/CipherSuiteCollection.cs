// Secure Sockets Layer / Transport Security Layer Implementation
// Copyright(c) 2003-2005 Carlos Guzman Alvarez

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
using System.Collections;
using System.Globalization;
using System.Security.Authentication;
using System.Security.Cryptography;

namespace SecureSocketLayer.Net.Security.Providers.Common
{
	internal sealed class CipherSuiteCollection : CollectionBase
	{
		#region · Fields ·

		private SslProtocols protocol;

		#endregion

		#region · Indexers ·

		public CipherSuite this[string name]
		{
			get { return(CipherSuite)this.List[this.List.IndexOf(name)]; }
			set { this.List[this.IndexOf(name)] = (CipherSuite)value; }
		}

		public CipherSuite this[int index]
		{
			get { return(CipherSuite)this.List[index]; }
			set { this.List[index] = (CipherSuite)value; }
		}

		public CipherSuite this[short code]
		{
			get { return(CipherSuite)this.List[this.IndexOf(code)]; }
			set { this.List[this.IndexOf(code)] = (CipherSuite)value; }
		}

		#endregion

		#region · Constructors ·

		public CipherSuiteCollection(SslProtocols protocol) : base()
		{
			this.protocol = protocol;
		}

		#endregion

		#region · Methods ·

		public CipherSuite Add(
			short code,
			string name,
			CipherAlgorithmType cipherType,
			HashAlgorithmType hashType,
			ExchangeAlgorithmType exchangeType,
			bool exportable,
			bool blockMode,
			byte keyMaterialSize,
			byte expandedKeyMaterialSize,
			short effectiveKeyBytes,
			byte ivSize,
			byte blockSize)
		{
			CipherSuite cipher = new CipherSuite(
				code,
				name,
				cipherType,
				hashType,
				exchangeType,
				exportable,
				blockMode,
				keyMaterialSize,
				expandedKeyMaterialSize,
				effectiveKeyBytes,
				ivSize,
				blockSize);

			this.Add(cipher);

			return cipher;
		}

		public int IndexOf(string name)
		{
			int index = 0;

			foreach (CipherSuite cipherSuite in this.List)
			{
				if (cipherSuite.Name == name)
				{
					return index;
				}
				index++;
			}

			return -1;
		}

		public int IndexOf(short code)
		{
			int index = 0;

			foreach (CipherSuite cipherSuite in this.List)
			{
				if (cipherSuite.Code == code)
				{
					return index;
				}
				index++;
			}

			return -1;
		}

		public int IndexOf(CipherSuite value)
		{
			return this.List.IndexOf(value);
		}

		public void Remove(CipherSuite value)
		{
			this.List.Remove(value);
		}

		public bool Contains(CipherSuite value)
		{
			return this.List.Contains(value);
		}

		#endregion

		#region · Private Methods ·

		private int Add(CipherSuite cipherSuite)
		{
			return this.List.Add(cipherSuite);
		}

		#endregion
	}
}

#endif

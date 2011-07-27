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
using System.IO;
using System.Text;
using System.Security.Authentication;
using System.Security.Cryptography;

using Mono.Security.Cryptography;

namespace SecureSocketLayer.Net.Security.Providers.Common
{
	internal sealed class CipherSuite
	{
		#region · Fields ·

		private short                   code;
		private string                  name;
		private CipherAlgorithmType     cipherAlgorithmType;
		private HashAlgorithmType       hashAlgorithmType;
		private ExchangeAlgorithmType   exchangeAlgorithmType;
		private bool                    isExportable;
		private CipherMode              cipherMode;
		private byte                    keyMaterialSize;
		private int                     keyBlockSize;
		private byte                    expandedKeyMaterialSize;
		private short                   effectiveKeyBits;
		private byte                    ivSize;
		private byte                    blockSize;

		#endregion

		#region · Properties ·

		public CipherAlgorithmType CipherAlgorithmType
		{
			get { return this.cipherAlgorithmType; }
		}

		public string HashAlgorithmName
		{
			get
			{
				switch (this.hashAlgorithmType) {
					case HashAlgorithmType.Md5:
						return "MD5";

					case HashAlgorithmType.Sha1:
						return "SHA1";

					default:
						return "None";
				}
			}
		}

		public HashAlgorithmType HashAlgorithmType
		{
			get { return this.hashAlgorithmType; }
		}

		public int HashSize
		{
			get
			{
				switch (this.hashAlgorithmType) 
                {
					case HashAlgorithmType.Md5:
						return 16;

					case HashAlgorithmType.Sha1:
						return 20;

					default:
						return 0;
				}
			}
		}

		public ExchangeAlgorithmType ExchangeAlgorithmType
		{
			get { return this.exchangeAlgorithmType; }
		}

		public CipherMode CipherMode
		{
			get { return this.cipherMode; }
		}

		public short Code
		{
			get { return this.code; }
		}

		public string Name
		{
			get { return this.name; }
		}

		public bool IsExportable
		{
			get { return this.isExportable; }
		}

		public byte KeyMaterialSize
		{
			get { return this.keyMaterialSize; }
		}

		public int KeyBlockSize
		{
			get { return this.keyBlockSize; }
		}

		public byte ExpandedKeyMaterialSize
		{
			get { return this.expandedKeyMaterialSize; }
		}

		public short EffectiveKeyBits
		{
			get { return this.effectiveKeyBits; }
		}

		public byte IvSize
		{
			get { return this.ivSize; }
		}

		public byte BlockSize
		{
			get { return this.blockSize; }
		}

		#endregion

		#region · Constructors ·

		public CipherSuite(
			short                   code,
			string                  name,
			CipherAlgorithmType     cipherAlgorithmType,
			HashAlgorithmType       hashAlgorithmType,
			ExchangeAlgorithmType   exchangeAlgorithmType,
			bool                    exportable,
			bool                    blockMode,
			byte                    keyMaterialSize,
			byte                    expandedKeyMaterialSize,
            short                   effectiveKeyBits,
			byte                    ivSize,
			byte                    blockSize)
		{
			this.code                   = code;
			this.name                   = name;
			this.cipherAlgorithmType    = cipherAlgorithmType;
			this.hashAlgorithmType      = hashAlgorithmType;
			this.exchangeAlgorithmType  = exchangeAlgorithmType;
			this.isExportable           = exportable;

            if (blockMode)
            {
                this.cipherMode = CipherMode.CBC;
            }

			this.keyMaterialSize            = keyMaterialSize;
			this.expandedKeyMaterialSize    = expandedKeyMaterialSize;
            this.effectiveKeyBits           = effectiveKeyBits;
			this.ivSize                     = ivSize;
			this.blockSize                  = blockSize;
			this.keyBlockSize               = (this.keyMaterialSize + this.HashSize + this.ivSize) << 1;
		}

		#endregion
	}
}

#endif
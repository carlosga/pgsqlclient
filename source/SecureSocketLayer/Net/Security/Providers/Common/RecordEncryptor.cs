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

using Mono.Security.Cryptography;
using System.IO;
using System.Security.Authentication;
using System.Security.Cryptography;

namespace SecureSocketLayer.Net.Security.Providers.Common
{
	internal abstract class RecordEncryptor : IRecordEncryptor
	{		
		#region · Fields ·

		private CipherSuite			cipherSuite;
		private ISecureKeyInfo		keyInfo;
		private SymmetricAlgorithm	encryptionAlgorithm;
		private ICryptoTransform	encryptionCipher;

		#endregion

		#region · Protected Properties ·

		protected CipherSuite CipherSuite
		{
			get { return this.cipherSuite; }
		}

		protected ISecureKeyInfo KeyInfo
		{
			get { return this.keyInfo; }
		}

		protected SymmetricAlgorithm EncryptionAlgorithm
		{
			get { return this.encryptionAlgorithm; }
			set { this.encryptionAlgorithm = value; }
		}

		protected ICryptoTransform EncryptionCipher
		{
			get { return this.encryptionCipher; }
			set { this.encryptionCipher = value; }
		}

		#endregion

		#region · Protected Constructors ·

		protected RecordEncryptor(CipherSuite cipherSuite, ISecureKeyInfo keyInfo)
		{
			this.cipherSuite	= cipherSuite;
			this.keyInfo		= keyInfo;

			this.Initialize();
		}

		#endregion

		#region · Methods ·

		public virtual void UpdateClientCipherIV(byte[] iv)
		{
			if (this.CipherSuite.CipherMode == CipherMode.CBC) 
			{
				// Set the new IV
				this.EncryptionAlgorithm.IV = iv;

				// Create encryption cipher with the new IV
				this.EncryptionCipher = this.encryptionAlgorithm.CreateEncryptor();
			}		
		}

		public virtual byte[] Encrypt(byte[] fragment, byte[] mac)
		{
			// Encryption( fragment + mac [+ padding + padding_length] )
			MemoryStream ms = new MemoryStream();
			CryptoStream cs = new CryptoStream(ms, this.EncryptionCipher, CryptoStreamMode.Write);

			cs.Write(fragment, 0, fragment.Length);
			cs.Write(mac, 0, mac.Length);
			if (this.CipherSuite.CipherMode == CipherMode.CBC) 
			{
				// Calculate padding_length
				byte fragmentLength = (byte)(fragment.Length + mac.Length + 1);
				byte paddingLength = (byte)(this.CipherSuite.BlockSize - fragmentLength % this.CipherSuite.BlockSize);
				if (paddingLength == this.CipherSuite.BlockSize) 
				{
					paddingLength = 0;
				}

				// Write padding length byte
				byte[] padding = new byte[(paddingLength + 1)];
				for (int i = 0; i < (paddingLength + 1); i++) 
				{
					padding[i] = paddingLength;
				}

				cs.Write(padding, 0, padding.Length);
			}

			cs.FlushFinalBlock();
			cs.Close();

			return ms.ToArray();
		}

		#endregion

		#region · Protected Methods ·

		protected virtual void Initialize()
		{
			// Create and configure the symmetric algorithm
			switch (this.CipherSuite.CipherAlgorithmType) 
			{
				case CipherAlgorithmType.Aes:
				case CipherAlgorithmType.Aes128:
				case CipherAlgorithmType.Aes192:
				case CipherAlgorithmType.Aes256:
					this.EncryptionAlgorithm = Rijndael.Create();
					break;

				case CipherAlgorithmType.TripleDes:
					this.EncryptionAlgorithm = TripleDES.Create();
					break;

				case CipherAlgorithmType.Des:
					this.EncryptionAlgorithm = DES.Create();
					break;

				case CipherAlgorithmType.Rc4:
					this.EncryptionAlgorithm = new ARC4Managed();
					break;

				case CipherAlgorithmType.Rc2:
					this.EncryptionAlgorithm = RC2.Create();
					break;
			}

			// If it's a block cipher
			if (this.CipherSuite.CipherMode == CipherMode.CBC) 
			{
				// Configure encrypt algorithm
				this.EncryptionAlgorithm.Mode		= this.CipherSuite.CipherMode;
				this.EncryptionAlgorithm.Padding	= PaddingMode.None;
				this.EncryptionAlgorithm.KeySize	= this.CipherSuite.ExpandedKeyMaterialSize * 8;
				this.EncryptionAlgorithm.BlockSize	= this.CipherSuite.BlockSize * 8;
			}		
		}

		#endregion
	}
}

#endif
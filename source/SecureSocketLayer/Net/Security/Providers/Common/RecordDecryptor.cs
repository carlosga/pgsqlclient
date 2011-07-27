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
using System.Security.Authentication;
using System.Security.Cryptography;
using Mono.Security.Cryptography;

namespace SecureSocketLayer.Net.Security.Providers.Common
{
	internal abstract class RecordDecryptor : IRecordDecryptor
	{
		#region · Fields ·

		private CipherSuite			cipherSuite;
		private ISecureKeyInfo		keyInfo;
		private SymmetricAlgorithm	decryptionAlgorithm;
		private ICryptoTransform	decryptionCipher;

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

		protected SymmetricAlgorithm DecryptionAlgorithm
		{
			get { return this.decryptionAlgorithm; }
			set { this.decryptionAlgorithm = value; }
		}

		protected ICryptoTransform DecryptionCipher
		{
			get { return this.decryptionCipher; }
			set { this.decryptionCipher = value; }
		}

		#endregion

		#region · Protected Constructors ·

		protected RecordDecryptor(CipherSuite cipherSuite, ISecureKeyInfo keyInfo)
		{
			this.cipherSuite	= cipherSuite;
			this.keyInfo		= keyInfo;

			this.Initialize();
		}

		#endregion

		#region · Methods ·

		public void UpdateClientCipherIV(byte[] iv)
		{
			if (this.CipherSuite.CipherMode == CipherMode.CBC) 
			{
				// Set the new IV
				this.DecryptionAlgorithm.IV = iv;

				// Create encryption cipher with the new IV
				this.DecryptionCipher = this.DecryptionAlgorithm.CreateEncryptor();
			}		
		}

		public void Decrypt(byte[] fragment, ref byte[] dcrFragment, ref byte[] dcrMAC)
		{
			int fragmentSize = 0;
			int paddingLength = 0;

			// Decrypt message fragment( fragment + mac [+ padding + padding_length] )
			byte[] buffer = new byte[fragment.Length];
			this.DecryptionCipher.TransformBlock(fragment, 0, fragment.Length, buffer, 0);

			// Calculate fragment size
			if (this.CipherSuite.CipherMode == CipherMode.CBC) 
			{
				// Calculate padding_length
				paddingLength = buffer[buffer.Length - 1];
				fragmentSize = (buffer.Length - (paddingLength + 1)) - this.CipherSuite.HashSize;
			}
			else 
			{
				fragmentSize = buffer.Length - this.CipherSuite.HashSize;
			}

			dcrFragment = new byte[fragmentSize];
			dcrMAC = new byte[this.CipherSuite.HashSize];

			Buffer.BlockCopy(buffer, 0, dcrFragment, 0, dcrFragment.Length);
			Buffer.BlockCopy(buffer, dcrFragment.Length, dcrMAC, 0, dcrMAC.Length);
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
					this.DecryptionAlgorithm = Rijndael.Create();
					break;

				case CipherAlgorithmType.TripleDes:
					this.DecryptionAlgorithm = TripleDES.Create();
					break;

				case CipherAlgorithmType.Des:
					this.DecryptionAlgorithm = DES.Create();
					break;

				case CipherAlgorithmType.Rc4:
					this.DecryptionAlgorithm = new ARC4Managed();
					break;

				case CipherAlgorithmType.Rc2:
					this.DecryptionAlgorithm = RC2.Create();
					break;
			}

			// If it's a block cipher
			if (this.CipherSuite.CipherMode == CipherMode.CBC) 
			{
				// Configure encrypt algorithm
				this.DecryptionAlgorithm.Mode = this.CipherSuite.CipherMode;
				this.DecryptionAlgorithm.Padding = PaddingMode.None;
				this.DecryptionAlgorithm.KeySize = this.CipherSuite.ExpandedKeyMaterialSize * 8;
				this.DecryptionAlgorithm.BlockSize = this.CipherSuite.BlockSize * 8;
			}		
		}

		#endregion
	}
}

#endif
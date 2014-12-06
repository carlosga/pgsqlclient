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
using System;

namespace SecureSocketLayer.Net.Security.Providers.Tls
{
	internal sealed class TlsSecureKeyGenerator : SecureKeyGenerator
	{
		#region · Constructors ·

		public TlsSecureKeyGenerator(SecureSession session) : base(session)
		{
		}

		#endregion

		#region · ISecureKeyGenerator Methods ·

		public override void Generate(ref ISecureKeyInfo keyInfo)
		{
			CipherSuite	cipherSuite = this.Session.CipherSuite;

			// Set the premaster and master secrets
			keyInfo.PreMasterSecret = this.CreatePremasterSecret();
			keyInfo.MasterSecret	= this.CreateMasterSecret(keyInfo);

			// Create keyblock
			MemoryStreamEx keyBlock = new MemoryStreamEx(
				Helper.PseudoRandomFunction(keyInfo.MasterSecret, "key expansion", keyInfo.ServerClientRandom, cipherSuite.KeyBlockSize));

			keyInfo.ClientWriteMAC = keyBlock.ReadBytes(cipherSuite.HashSize);
			keyInfo.ServerWriteMAC = keyBlock.ReadBytes(cipherSuite.HashSize);
			keyInfo.ClientWriteKey = keyBlock.ReadBytes(cipherSuite.KeyMaterialSize);
			keyInfo.ServerWriteKey = keyBlock.ReadBytes(cipherSuite.KeyMaterialSize);

			if (!cipherSuite.IsExportable) 
			{
				if (cipherSuite.IvSize != 0) 
				{
					keyInfo.ClientWriteIV = keyBlock.ReadBytes(cipherSuite.IvSize);
					keyInfo.ServerWriteIV = keyBlock.ReadBytes(cipherSuite.IvSize);
				}
				else 
				{
					keyInfo.ClientWriteIV = EmptyArray;
					keyInfo.ServerWriteIV = EmptyArray;
				}
			}
			else 
			{
				// Generate final write keys
				byte[] finalClientWriteKey = Helper.PseudoRandomFunction(keyInfo.ClientWriteKey, "client write key", keyInfo.ClientServerRandom, cipherSuite.ExpandedKeyMaterialSize);
				byte[] finalServerWriteKey = Helper.PseudoRandomFunction(keyInfo.ServerWriteKey, "server write key", keyInfo.ClientServerRandom, cipherSuite.ExpandedKeyMaterialSize);

				keyInfo.ClientWriteKey = finalClientWriteKey;
				keyInfo.ServerWriteKey = finalServerWriteKey;

				if (cipherSuite.IvSize > 0) 
				{
					// Generate IV block
					byte[] ivBlock = Helper.PseudoRandomFunction(EmptyArray, "IV block", keyInfo.ClientServerRandom, cipherSuite.IvSize * 2);

					// Generate IV keys
					keyInfo.ClientWriteIV = new byte[cipherSuite.IvSize];
					Buffer.BlockCopy(ivBlock, 0, keyInfo.ClientWriteIV, 0, keyInfo.ClientWriteIV.Length);

					keyInfo.ServerWriteIV = new byte[cipherSuite.IvSize];
					Buffer.BlockCopy(ivBlock, cipherSuite.IvSize, keyInfo.ServerWriteIV, 0, keyInfo.ServerWriteIV.Length);
				}
				else 
				{
					keyInfo.ClientWriteIV = EmptyArray;
					keyInfo.ServerWriteIV = EmptyArray;
				}
			}

			// Clear no more needed data
			keyBlock.Close();
		}

		#endregion

		#region · Key Generation Methods ·

		private byte[] CreateMasterSecret(ISecureKeyInfo keyInfo)
		{
			return Helper.PseudoRandomFunction(keyInfo.PreMasterSecret, "master secret", keyInfo.ClientServerRandom, 48);
		}

		#endregion
	}
}

#endif
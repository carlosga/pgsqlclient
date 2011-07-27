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
using SecureSocketLayer.Net.Security.Providers.Common;
using System.Security.Cryptography;

namespace SecureSocketLayer.Net.Security.Providers.Ssl
{
	internal sealed class SslSecureKeyGenerator : SecureKeyGenerator
	{
		#region · Constructors ·

		public SslSecureKeyGenerator(SecureSession session) : base(session)
		{
		}

		#endregion

		#region · ISecureKeyGenerator Methods ·

		public override void Generate(ref ISecureKeyInfo keyInfo)
		{
			CipherSuite		cipher			= this.Session.CipherSuite;
			MemoryStreamEx	tmp				= new MemoryStreamEx();
			char			labelChar		= 'A';
			int				count			= 1;			

			keyInfo.PreMasterSecret = this.CreatePremasterSecret();
			keyInfo.MasterSecret	= this.CreateMasterSecret(keyInfo);

			while (tmp.Length < cipher.KeyBlockSize) 
			{
				string label = String.Empty;

				for (int i = 0; i < count; i++) 
				{
					label += labelChar.ToString();
				}

				byte[] block = Helper.PseudoRandomFunction(keyInfo.MasterSecret, label.ToString(), keyInfo.ServerClientRandom);

				int size = (tmp.Length + block.Length) > cipher.KeyBlockSize ? (cipher.KeyBlockSize - (int)tmp.Length) : block.Length;

				tmp.Write(block, 0, size);

				labelChar++;
				count++;
			}

			// Create keyblock
			MemoryStreamEx keyBlock = new MemoryStreamEx(tmp.ToArray());

			keyInfo.ClientWriteMAC = keyBlock.ReadBytes(cipher.HashSize);
			keyInfo.ServerWriteMAC = keyBlock.ReadBytes(cipher.HashSize);
			keyInfo.ClientWriteKey = keyBlock.ReadBytes(cipher.KeyMaterialSize);
			keyInfo.ServerWriteKey = keyBlock.ReadBytes(cipher.KeyMaterialSize);

			if (!cipher.IsExportable) 
			{
				if (cipher.IvSize != 0) 
				{
					keyInfo.ClientWriteIV = keyBlock.ReadBytes(cipher.IvSize);					
					keyInfo.ServerWriteIV = keyBlock.ReadBytes(cipher.IvSize);
				}
				else 
				{
					keyInfo.ClientWriteIV = EmptyArray;
					keyInfo.ServerWriteIV = EmptyArray;
				}
			}
			else 
			{
				HashAlgorithm md5 = MD5.Create();

				int keySize = (md5.HashSize >> 3); //in bytes not bits
				byte[] temp = new byte[keySize];

				// Generate final write keys
				md5.TransformBlock(keyInfo.ClientWriteKey, 0, keyInfo.ClientWriteKey.Length, temp, 0);
				md5.TransformFinalBlock(keyInfo.ClientServerRandom, 0, keyInfo.ClientServerRandom.Length);
				byte[] finalClientWriteKey = new byte[cipher.ExpandedKeyMaterialSize];
				Buffer.BlockCopy(md5.Hash, 0, finalClientWriteKey, 0, cipher.ExpandedKeyMaterialSize);

				md5.Initialize();
				md5.TransformBlock(keyInfo.ServerWriteKey, 0, keyInfo.ServerWriteKey.Length, temp, 0);
				md5.TransformFinalBlock(keyInfo.ServerClientRandom, 0, keyInfo.ServerClientRandom.Length);
				byte[] finalServerWriteKey = new byte[cipher.ExpandedKeyMaterialSize];
				Buffer.BlockCopy(md5.Hash, 0, finalServerWriteKey, 0, cipher.ExpandedKeyMaterialSize);

				keyInfo.ClientWriteKey = finalClientWriteKey;
				keyInfo.ServerWriteKey = finalServerWriteKey;

				// Generate IV keys
				if (cipher.IvSize > 0) 
				{
					md5.Initialize();
					temp = md5.ComputeHash(keyInfo.ClientServerRandom, 0, keyInfo.ClientServerRandom.Length);
					keyInfo.ClientWriteIV = new byte[cipher.IvSize];
					Buffer.BlockCopy(temp, 0, keyInfo.ClientWriteIV, 0, cipher.IvSize);

					md5.Initialize();
					temp = md5.ComputeHash(keyInfo.ServerClientRandom, 0, keyInfo.ServerClientRandom.Length);
					keyInfo.ServerWriteIV = new byte[cipher.IvSize];
					Buffer.BlockCopy(temp, 0, keyInfo.ServerWriteIV, 0, cipher.IvSize);
				}
				else 
				{
					keyInfo.ClientWriteIV = EmptyArray;
					keyInfo.ServerWriteIV = EmptyArray;
				}
			}

			// Clear no more needed data
			keyBlock.Close();
			tmp.Close();
		}

		#endregion

		#region · Key Generation Methods ·

		private byte[] CreateMasterSecret(ISecureKeyInfo keyInfo)
		{
			MemoryStreamEx masterSecret = new MemoryStreamEx();

			masterSecret.Write(Helper.PseudoRandomFunction(keyInfo.PreMasterSecret, "A", keyInfo.ClientServerRandom));
			masterSecret.Write(Helper.PseudoRandomFunction(keyInfo.PreMasterSecret, "BB", keyInfo.ClientServerRandom));
			masterSecret.Write(Helper.PseudoRandomFunction(keyInfo.PreMasterSecret, "CCC", keyInfo.ClientServerRandom));

			return masterSecret.ToArray();
		}

		#endregion
	}
}

#endif
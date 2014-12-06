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

using SecureSocketLayer.Net.Security.Providers.Cryptography;
using System;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;

namespace SecureSocketLayer.Net.Security.Providers.Common
{
	internal sealed class Helper
	{
		#region · Constructors ·

		private Helper()
		{
		}

		#endregion

		#region · Static Methods ·

		public static byte[] GenerateRandom()
		{
			MemoryStreamEx random = new MemoryStreamEx();
			random.Write(Helper.GetUnixTime());
			random.Write(Helper.GetSecureRandomBytes(28));

			return random.ToArray();
		}

		public static int GetUnixTime()
		{
			DateTime now = DateTime.UtcNow;

			return(int)(now.Ticks - 621355968000000000 / TimeSpan.TicksPerSecond);
		}

		public static byte[] GetSecureRandomBytes(int count)
		{
			byte[] secureBytes = new byte[count];
			System.Security.Cryptography.RandomNumberGenerator random = System.Security.Cryptography.RandomNumberGenerator.Create();

			random.GetNonZeroBytes(secureBytes);

			return secureBytes;
		}

		public static short GetProtocolCode(SslProtocols protocol)
		{
			switch (protocol) 
			{
				case SslProtocols.Tls:
					return 769; //(0x03 << 8) | 0x01;

				case SslProtocols.Ssl3:
					return 768; //(0x03 << 8) | 0x00;

				default:
					throw new NotSupportedException("Unsupported security protocol type");
			}
		}

		public static SslProtocols GetProtocolType(short protocol)
		{
			switch (protocol) 
			{
				case 769:
					return SslProtocols.Tls;

				case 768:
					return SslProtocols.Ssl3;

				default:
					throw new NotSupportedException("Unsupported security protocol type");
			}
		}

		public static byte[] EncodeInt16(short value)
		{
			return BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder(value));
		}

		public static short DecodeInt16(byte[] buffer)
		{
			return IPAddress.HostToNetworkOrder(BitConverter.ToInt16(buffer, 0));
		}

		public static byte[] EncodeInt24(int value)
		{
			int int24 = IPAddress.HostToNetworkOrder(value);
			byte[] content = new byte[3];

			Buffer.BlockCopy(BitConverter.GetBytes(int24), 1, content, 0, 3);

			return content;
		}

		public static int DecodeInt24(byte[] buffer)
		{
			return((buffer[0] & 0xff) << 16) |((buffer[1] & 0xff) << 8) |(buffer[2] & 0xff);
		}

		public static bool Compare(byte[] buffer1, byte[] buffer2)
		{
			if (buffer1.Length != buffer2.Length)
			{
				return false;
			}
			else
			{
				for (int i = 0; i < buffer1.Length; i++)
				{
					if (buffer1[i] != buffer2[i])
					{
						return false;
					}
				}
			}

			return true;
		}

		#endregion

		#region · Pseudo-Random Function Implementation Static Methods ·

		public static byte[] PseudoRandomFunction(byte[] secret, string label, byte[] random)
		{
			HashAlgorithm md5 = MD5.Create();
			HashAlgorithm sha = SHA1.Create();

			// Compute SHA hash
			MemoryStreamEx block = new MemoryStreamEx();
			block.Write(Encoding.ASCII.GetBytes(label));
			block.Write(secret);
			block.Write(random);
						
			byte[] shaHash = sha.ComputeHash(block.ToArray(), 0,(int)block.Length);

			block.Reset();

			// Compute MD5 hash
			block.Write(secret);
			block.Write(shaHash);

			byte[] result = md5.ComputeHash(block.ToArray(), 0,(int)block.Length);

			// Free resources
			block.Reset();

			return result;
		}

		public static byte[] PseudoRandomFunction(byte[] secret, string label, byte[] data, int length)
		{
			/* Secret Length calc exmplain from the RFC2246. Section 5
			 * 
			 * S1 and S2 are the two halves of the secret and each is the same
			 * length. S1 is taken from the first half of the secret, S2 from the
			 * second half. Their length is created by rounding up the length of the
			 * overall secret divided by two; thus, if the original secret is an odd
			 * number of bytes long, the last byte of S1 will be the same as the
			 * first byte of S2.
			 */

			// split secret in 2
			int secretLen = secret.Length >> 1;
			// rounding up
			if ((secret.Length & 0x1) == 0x1)
			{
				secretLen++;
			}

			// Seed
			MemoryStreamEx seedStream = new MemoryStreamEx();
			seedStream.Write(Encoding.ASCII.GetBytes(label));
			seedStream.Write(data);
			byte[] seed = seedStream.ToArray();
			seedStream.Reset();

			// Secret 1
			byte[] secret1 = new byte[secretLen];
			Buffer.BlockCopy(secret, 0, secret1, 0, secretLen);

			// Secret2
			byte[] secret2 = new byte[secretLen];
			Buffer.BlockCopy(secret,(secret.Length - secretLen), secret2, 0, secretLen);

			// Secret 1 processing
			byte[] p_md5 = Expand("MD5", secret1, seed, length);

			// Secret 2 processing
			byte[] p_sha = Expand("SHA1", secret2, seed, length);

			// Perfor XOR of both results
			byte[] masterSecret = new byte[length];
			for (int i = 0; i < masterSecret.Length; i++)
			{
				masterSecret[i] = (byte)(p_md5[i] ^ p_sha[i]);
			}

			return masterSecret;
		}

		public static byte[] Expand(string hashName, byte[] secret, byte[] seed, int length)
		{
			int hashLength	= hashName == "MD5" ? 16 : 20;
			int	iterations	= (int)(length / hashLength);
			if ((length % hashLength) > 0)
			{
				iterations++;
			}
			
			KeyedHashAlgorithm	hmac	= new HMACSsl(hashName, secret);
			MemoryStreamEx		resMacs	= new MemoryStreamEx();
			
			byte[][] hmacs = new byte[iterations + 1][];
			hmacs[0] = seed;
			for (int i = 1; i <= iterations; i++)
			{				
				MemoryStreamEx hcseed = new MemoryStreamEx();
				hmac.TransformFinalBlock(hmacs[i-1], 0, hmacs[i-1].Length);
				hmacs[i] = hmac.Hash;
				hcseed.Write(hmacs[i]);
				hcseed.Write(seed);
				hmac.TransformFinalBlock(hcseed.ToArray(), 0,(int)hcseed.Length);
				resMacs.Write(hmac.Hash);
				hcseed.Reset();
			}

			byte[] res = new byte[length];
			
			Buffer.BlockCopy(resMacs.ToArray(), 0, res, 0, res.Length);

			resMacs.Reset();

			return res;
		}

		#endregion
	}
}


#endif
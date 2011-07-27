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

namespace SecureSocketLayer.Net.Security.Providers.Common
{
	internal sealed class SecureKeyInfo : ISecureKeyInfo
	{
		#region · Fields ·

		private byte[] clientRandom;
		private byte[] serverRandom;		
		private byte[] clientServerRandom;
		private byte[] serverClientRandom;
		private byte[] preMasterSecret;
		private byte[] masterSecret;
		private byte[] clientWriteMAC;
		private byte[] serverWriteMAC;
		private byte[] clientWriteKey;
		private byte[] serverWriteKey;
		private byte[] clientWriteIV;
		private byte[] serverWriteIV;

		#endregion

		#region · Properties ·

		public byte[] ClientRandom 
		{ 
			get { return this.clientRandom; } 
			set 
			{ 
				this.clientRandom = value; 
				this.Update();
			}
		}

		public byte[] ServerRandom
		{ 
			get { return this.serverRandom; } 
			set 
			{ 
				this.serverRandom = value; 
				this.Update();
			}
		}

		public byte[] ClientServerRandom
		{ 
			get { return this.clientServerRandom;  }
		}

		public byte[] ServerClientRandom
		{ 
			get { return this.serverClientRandom; }
		}

		public byte[] PreMasterSecret
		{
			get { return this.preMasterSecret; }
			set { this.preMasterSecret = value; }
		}

		public byte[] MasterSecret
		{
			get { return this.masterSecret; }
			set { this.masterSecret = value; }
		}

		public byte[] ClientWriteMAC
		{ 
			get { return this.clientWriteMAC; } 
			set { this.clientWriteMAC = value;} 
		}

		public byte[] ServerWriteMAC
		{ 
			get { return this.serverWriteMAC; } 
			set { this.serverWriteMAC = value;} 
		}

		public byte[] ClientWriteKey
		{ 
			get { return this.clientWriteKey; } 
			set { this.clientWriteKey = value;} 
		}

		public byte[] ServerWriteKey
		{ 
			get { return this.serverWriteKey; } 
			set { this.serverWriteKey = value;} 
		}

		public byte[] ClientWriteIV
		{ 
			get { return this.clientWriteIV; } 
			set { this.clientWriteIV = value;} 
		}

		public byte[] ServerWriteIV	
		{ 
			get { return this.serverWriteIV; } 
			set { this.serverWriteIV = value; } 
		}

		#endregion

		#region · Constructors ·

		public SecureKeyInfo()
		{
		}

		#endregion

		#region · Private Methods ·

		private void Update()
		{
			if (this.ServerRandom != null && this.clientRandom != null)
			{
				// Random Client-Server
				MemoryStreamEx cs = new MemoryStreamEx();
				cs.Write(this.ClientRandom);
				cs.Write(this.ServerRandom);

				this.clientServerRandom = cs.ToArray();

				// Random Server-Client
				MemoryStreamEx sc = new MemoryStreamEx();
				sc.Write(this.ServerRandom);
				sc.Write(this.ClientRandom);

				this.serverClientRandom = sc.ToArray();
			}
		}

		#endregion
	}
}

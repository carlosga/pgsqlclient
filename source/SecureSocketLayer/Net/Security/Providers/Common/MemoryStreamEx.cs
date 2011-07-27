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
using System.IO;
using System.Net;

namespace SecureSocketLayer.Net.Security.Providers.Common
{
	internal sealed class MemoryStreamEx
	{
		#region · Fields ·

		private MemoryStream buffer;

		#endregion

		#region · Properties ·

		public long Position
		{
			get { return this.buffer.Position; }
			set { this.buffer.Position = value; }
		}

		public int Length
		{
			get { return(int)this.buffer.Length; }
		}

		public bool EOF
		{
			get
			{
				if (this.Position < this.Length) 
                {
					return false;
				}
				else 
                {
					return true;
				}
			}
		}

		#endregion

		#region · Constructors ·

		public MemoryStreamEx() : base()
		{
			this.buffer	= new MemoryStream();
		}

		public MemoryStreamEx(byte[] buffer)
		{
			this.buffer	= new MemoryStream(buffer);
		}

		#endregion

		#region · Specific Read Methods ·

		public byte ReadByte()
		{
			return(byte)this.buffer.ReadByte();
		}

		public short ReadInt16()
		{
			byte[] bytes = this.ReadBytes(2);

			return IPAddress.HostToNetworkOrder(BitConverter.ToInt16(bytes, 0));
		}

		public int ReadInt24()
		{
			return Helper.DecodeInt24(this.ReadBytes(3));
		}

		public int ReadInt32()
		{
			byte[] bytes = this.ReadBytes(4);

			return IPAddress.HostToNetworkOrder(BitConverter.ToInt32(bytes, 0));
		}

		public long ReadInt64()
		{
			byte[] bytes = this.ReadBytes(8);

			return IPAddress.HostToNetworkOrder(BitConverter.ToInt64(bytes, 0));
		}

		public byte[] ReadBytes(int count)
		{
			byte[] bytes = new byte[count];
			this.buffer.Read(bytes, 0, count);

			return bytes;
		}

		#endregion

		#region · Specific Write Methods ·

		public void Write(byte value)
		{
			this.buffer.WriteByte(value);
		}

		public void Write(short value)
		{
			byte[] bytes = BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder(value));
			this.Write(bytes);
		}

		public void WriteInt24(int value)
		{
			this.Write(Helper.EncodeInt24(value));
		}

		public void Write(int value)
		{
			byte[] bytes = BitConverter.GetBytes((int)IPAddress.HostToNetworkOrder(value));
			this.Write(bytes);
		}

		public void Write(long value)
		{
			byte[] bytes = BitConverter.GetBytes((long)IPAddress.HostToNetworkOrder(value));
			this.Write(bytes);
		}

		public void Write(byte[] buffer)
		{
			this.buffer.Write(buffer, 0, buffer.Length);
		}

		#endregion

		#region · Methods ·

		public void Flush()
		{
			this.buffer.Flush();
		}

		public void SetLength(long length)
		{
			this.buffer.SetLength(length);
		}

		public int Read(byte[] buffer, int offset, int count)
		{
            if (!this.buffer.CanRead)
            {
                throw new InvalidOperationException("Read operations are not allowed by this stream");
            }
			
			return this.buffer.Read(buffer, offset, count);			
		}

		public void Write(byte[] buffer, int offset, int count)
		{
            if (!this.buffer.CanWrite)
            {
                throw new InvalidOperationException("Write operations are not allowed by this stream");
            }

			this.buffer.Write(buffer, offset, count);
		}

		public void Close()
		{
			this.buffer.Close();
		}

		public void Reset()
		{
			this.buffer.SetLength(0);
			this.buffer.Position = 0;
		}

		public byte[] ToArray()
		{
			return this.buffer.ToArray();
		}

		#endregion
	}
}

#endif
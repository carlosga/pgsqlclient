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
using System.IO;

namespace SecureSocketLayer.Net.Security.Providers.Common
{
	internal abstract class SecureProtocol : ISecureProtocol
	{
		#region · Callbacks Fields ·

		private RecordReceivedCallback recordReceived;
		private RecordSentCallback recordSent;

		#endregion

		#region · Callbacks Properties ·

		public RecordReceivedCallback RecordReceived 
		{ 
			get { return this.recordReceived; }
			set { this.recordReceived = value; } 
		}

		public RecordSentCallback RecordSent 
		{ 
			get { return this.recordSent; }
			set { this.recordSent = value; } 
		}

		#endregion

		#region · Fields ·

		private SecureSession	session;
		private	Stream			inputStream;
		private	Stream			outputStream;
		private int				maxRecordLength;
		private long			writeSequenceNumber;
		private long			readSequenceNumber;

		#endregion

		#region · Properties ·

		public int MaxRecordLength 
		{ 
			get { return this.maxRecordLength; }
			set { this.maxRecordLength = value; }
		}

		public long WriteSequenceNumber
		{
			get { return this.writeSequenceNumber; }
		}

		public long ReadSequenceNumber
		{
			get { return this.readSequenceNumber; }
		}

		public Stream InputStream 
		{ 
			get { return this.inputStream; }
			set { this.inputStream = value; } 
		}

		public Stream OutputStream 
		{ 
			get { return this.outputStream; }
			set { this.outputStream = value; } 
		}


		#endregion

		#region · Protected Properties ·

		protected SecureSession Session
		{
			get { return this.session; }
		}

		#endregion

		#region · Constructors ·

		protected SecureProtocol(SecureSession session)
		{
			this.session			= session;
			this.maxRecordLength	= 16384;
		}

		#endregion

        #region · ISecureProtocol Members ·

        public SecureRecord Read()
		{
			// Try to read the Record Content Type
			int type = this.inputStream.ReadByte();
			if (type == -1)
			{
				return null;
			}

			ContentType	contentType	= (ContentType)type;
			SecureRecord record = this.ReadRecord(type);
			if (record == null)
			{
				// record incomplete(at the moment)
				return null;
			}

			// Set content type of the record
			record.ContentType = contentType;

			// Decrypt message contents if needed
			if (contentType == ContentType.Alert && record.Fragment.Length == 2)
			{
			}
			else
			{
#warning FIXME !!! Handle Record Compression

				if (this.Session.IsEncrypted && contentType != ContentType.ChangeCipherSpec)
				{
					record.Fragment = this.Decrypt(record);
				}
			}

			if (this.RecordReceived != null && !this.Session.IsEncrypted)
			{
				this.RecordReceived(record);
			}

			// Process record
			switch (contentType)
			{
				case ContentType.Alert:
#warning FIXME !!! Alert processing
					break;

				case ContentType.ApplicationData:
				case ContentType.ChangeCipherSpec:
				case ContentType.Handshake:
					return record;

				default:
					if (contentType != (ContentType)0x80)
					{
						throw new Exception("Unknown record received from server.");
					}
					break;
			}

			return null;
		}

		public void Write(byte[] buffer)
		{
			if (!this.session.IsAuthenticated)
			{
				this.Write(ContentType.Handshake, buffer);
			}
			else
			{
				this.Write(ContentType.ApplicationData, buffer);
			}
		}

		public void Write(ContentType contentType, byte[] buffer)
		{
			SecureRecordCollection records = this.EncodeFragment(contentType, buffer);

			foreach (SecureRecord r in records)
			{
				this.outputStream.WriteByte((byte)r.ContentType);
				this.outputStream.Write(Helper.EncodeInt16(r.Protocol), 0, 2);
				this.outputStream.Write(Helper.EncodeInt16((short)r.Fragment.Length), 0, 2);
				this.outputStream.Write(r.Fragment, 0, r.Fragment.Length);
				this.outputStream.Flush();
			}
		}

		public IAsyncResult BeginRead(
			byte[]			buffer,
			int				offset,
			int				count,
			AsyncCallback	callback,
			object			state)
		{
			throw new NotImplementedException();
		}

		public int EndRead(IAsyncResult asyncResult)
		{
			throw new NotImplementedException();
		}

		public IAsyncResult BeginWrite(
			byte[]			buffer,
			int				offset,
			int				count,
			AsyncCallback	callback,
			object			state)
		{
			MemoryStream data = new MemoryStream();

			SecureRecordCollection records = this.EncodeFragment(ContentType.ApplicationData, buffer, offset, count);
			foreach (SecureRecord r in records)
			{
				data.WriteByte((byte)r.ContentType);
				data.Write(Helper.EncodeInt16(r.Protocol), 0, 2);
				data.Write(Helper.EncodeInt16((short)r.Fragment.Length), 0, 2);
				data.Write(r.Fragment, 0, r.Fragment.Length);
				data.Flush();
			}

			return this.outputStream.BeginWrite(data.ToArray(), 0, (int)data.ToArray().Length, callback, state);
		}
		
		public void EndWrite(IAsyncResult asyncResult)
		{
			this.outputStream.EndWrite(asyncResult);
		}

		#endregion

		#region · Cryptography Methods ·

		protected virtual SecureRecord ReadRecord(int contentType)
		{
			switch (contentType)
			{
				case 0x80:
#warning FIXME !!! Handle Client Hello V2
					throw new NotSupportedException();

				default:
					if (!Enum.IsDefined(typeof(ContentType),(ContentType)contentType))
					{
						throw new SecureException("Decode error");
					}
					return this.ReadRecord();
			}
		}

		private SecureRecord ReadRecord()
		{
			SecureRecord record = new SecureRecord();
			record.Protocol = this.ReadShort();

			short length = this.ReadShort();

			// process further only if the whole record is available
			// note: the first 5 bytes aren't part of the length
			if (this.inputStream.CanSeek && 
				(length + 5 > this.inputStream.Length)) 
			{
				return null;
			}
			
			// Read Record data
			int		received	= 0;
			byte[]	buffer		= new byte[length];
			while (received != length)
			{
				received += this.inputStream.Read(buffer, received, buffer.Length - received);
			}

			record.Fragment = buffer;

			return record;
		}

		protected virtual SecureRecordCollection EncodeFragment(
			ContentType	contentType, 
			byte[]		buffer)
		{
			return this.EncodeFragment(contentType, buffer, 0, buffer.Length);
		}

		protected virtual SecureRecordCollection EncodeFragment(
			ContentType	contentType, 
			byte[]		buffer,
			int			offset,
			int			count)
		{
			SecureRecordCollection	records		= new SecureRecordCollection();
			int						position	= offset;

			while (position < (offset + count))
			{
				int	fragmentLength = 0;

				if ((count - position) > this.MaxRecordLength)
				{
					fragmentLength = this.MaxRecordLength;
				}
				else
				{
					fragmentLength = count - position;
				}

				SecureRecord record = new SecureRecord();
				record.ContentType	= contentType;
				record.Protocol		= Helper.GetProtocolCode(this.session.CurrentProtocolType);
				record.Fragment		= new byte[fragmentLength];

				Buffer.BlockCopy(buffer, position, record.Fragment, 0, record.Fragment.Length);

				if (this.RecordSent != null)
				{
					// For the Handshake Message Hash we need the
					// original record (not encrypted)
					this.RecordSent(record);
				}

				if (this.session.IsEncrypted)
				{
					record.Fragment = this.Encrypt(record);
				}

				// Add the new record to the collection
				records.Add(record);

				// Update buffer position
				position += fragmentLength;
			}

			return records;
		}

		protected virtual byte[] Encrypt(SecureRecord record)
		{
			this.writeSequenceNumber++;

			return null;
		}

		protected virtual byte[] Decrypt(SecureRecord record)
		{
			this.readSequenceNumber++;

			return null;
		}

		#endregion

		#region · Private Methods ·

		private short ReadShort()
		{
			byte[] b = new byte[2];
			this.inputStream.Read(b, 0, b.Length);

			short val = BitConverter.ToInt16(b, 0);

			return System.Net.IPAddress.HostToNetworkOrder(val);
		}

		#endregion
	}
}

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

namespace SecureSocketLayer.Net.Security.Providers.Common
{
	internal interface ISecureProtocol
	{
		#region · Callbacks ·

		RecordReceivedCallback RecordReceived { get; set; }
		RecordSentCallback RecordSent { get; set; }

		#endregion

		#region · Properties ·

		int MaxRecordLength { get; set; }

		long WriteSequenceNumber { get; }

		long ReadSequenceNumber { get; }

		Stream InputStream { get; set; }

		Stream OutputStream { get; set; }

		#endregion

		#region · Methods ·

		SecureRecord Read();
		void Write(byte[] buffer);
		void Write(ContentType contentType, byte[] buffer);
		IAsyncResult BeginRead(
			byte[]			buffer,
			int				offset,
			int				count,
			AsyncCallback	callback,
			object			state);
		int EndRead(IAsyncResult asyncResult);
		IAsyncResult BeginWrite(
			byte[]			buffer,
			int				offset,
			int				count,
			AsyncCallback	callback,
			object			state);
		void EndWrite(IAsyncResult asyncResult);

		#endregion
	}
}

#endif
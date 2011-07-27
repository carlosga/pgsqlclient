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
using System.Collections;

namespace SecureSocketLayer.Net.Security.Providers.Common
{
	internal sealed class SecureRecordCollection : CollectionBase
	{
		#region · Indexers ·

		public SecureRecord this[ int index ]  
		{
			get { return(SecureRecord)this.List[index]; }
			set { this.List[index] = (SecureRecord)value; }
		}

		#endregion

		#region · Methods ·

		public int Add(SecureRecord value)  
		{
			return this.List.Add(value);
		}

		public int IndexOf(SecureRecord value)  
		{
			return this.List.IndexOf(value);
		}

		public void Insert(int index, SecureRecord value)
		{
			this.List.Insert(index, value);
		}

		public void Remove(SecureRecord value)  
		{
			this.List.Remove(value);
		}

		public bool Contains(SecureRecord value)  
		{
			return this.List.Contains(value);
		}

		#endregion
	}
}

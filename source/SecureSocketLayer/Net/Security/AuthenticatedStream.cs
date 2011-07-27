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

namespace SecureSocketLayer.Net.Security
{
    public abstract class AuthenticatedStream
        : Stream
    {
        #region · Fields ·

        private Stream innerStream;
        private bool leaveStreamOpen;

        #endregion

        #region · Properties ·

        public virtual bool LeaveInnerStreamOpen 
        {
            get { return this.leaveStreamOpen; }
        }

        #endregion

        #region · Protected Properties ·

        public Stream InnerStream 
        {
            get { return this.innerStream; }
        }

        #endregion

        #region · Abstract Properties ·

        public abstract bool IsAuthenticated { get; }
        public abstract bool IsEncrypted { get; }
        public abstract bool IsMutuallyAuthenticated { get; }
        public abstract bool IsServer { get; }
        public abstract bool IsSigned { get; }

        #endregion

        #region · Constructors ·

        protected AuthenticatedStream(Stream innerStream, bool leaveStreamOpen)
        {
            this.innerStream = innerStream;
            this.leaveStreamOpen = leaveStreamOpen;
        }

        #endregion

        #region · Methods ·

        public override void Close()
        {
            if (!this.leaveStreamOpen)
            {
                this.innerStream.Close();
            }

            this.innerStream        = null;
            this.leaveStreamOpen    = false;
        }

        #endregion
    }
}

#endif


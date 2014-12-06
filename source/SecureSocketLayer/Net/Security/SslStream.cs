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
using System.IO;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace SecureSocketLayer.Net.Security
{
    public class SslStream 
        : AuthenticatedStream
    {
        #region · Callback Fields ·

        private RemoteCertificateValidationCallback userCertificateValidationCallback;
        private LocalCertificateSelectionCallback userCertificateSelectionCallback;

        #endregion

        #region · Fields ·

        private Stream				innerStream;
        private MemoryStream		inputBuffer;
        private SecureSession		session;
        private object				read;
        private object				write;
        private bool				disposed;
        private object				syncObject;

        #endregion

        #region · Stream Properties ·

        public override bool CanRead 
        {
            get { return this.InnerStream.CanRead; }
        }

        public override bool CanSeek 
        {
            get { return false; }
        }

        public override bool CanWrite 
        {
            get { return this.InnerStream.CanWrite; }
        }

        public override long Length 
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position 
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        #endregion

        #region · Properties ·

        public override bool CanTimeout 
        {
            get { throw new NotImplementedException(); }
        }

        public virtual bool CheckCertRevocationStatus 
        {
            get { throw new NotImplementedException(); }
        }

        public virtual CipherAlgorithmType CipherAlgorithm 
        {
            get 
            {
                if (this.session != null && this.session.IsAuthenticated)
                {
                    return this.session.CipherSuite.CipherAlgorithmType;
                }
                return CipherAlgorithmType.None;
            }
        }

        public virtual int CipherStrength 
        {
            get 
            {
                if (this.session != null && this.session.IsAuthenticated)
                {
                    return this.session.CipherSuite.EffectiveKeyBits;
                }
                return 0;
            }
        }

        public virtual HashAlgorithmType HashAlgorithm 
        {
            get 
            {
                if (this.session != null && this.session.IsAuthenticated)
                {
                    return this.session.CipherSuite.HashAlgorithmType;
                }
                return HashAlgorithmType.None;
            }
        }

        public virtual int HashStrength 
        {
            get 
            {
                if (this.session != null && this.session.IsAuthenticated)
                {
                    return this.session.CipherSuite.HashSize * 8;
                }
                return 0;
            }
        }

        public override bool IsAuthenticated 
        {
            get { return (this.session != null && this.session.IsAuthenticated); }
        }

        public override bool IsEncrypted 
        {
            get 
            {
                if (this.session != null && this.session.IsAuthenticated)
                {
                    return this.session.IsEncrypted;
                }
                return false;
            }
        }

        public override bool IsMutuallyAuthenticated 
        {
            get 
            {
                if (this.session != null && this.session.IsAuthenticated)
                {
                    return this.session.IsMutuallyAuthenticated;
                }
                return false;
            }
        }

        public override bool IsServer 
        {
            get 
            {
                if (this.session != null && this.session.IsAuthenticated)
                {
                    return this.session.IsServer;
                }
                return false;
            }
        }

        public override bool IsSigned 
        {
            get 
            {
                if (this.session != null && this.session.IsAuthenticated)
                {
                    return this.session.IsSigned;
                }
                return false;
            }
        }

        public virtual ExchangeAlgorithmType KeyExchangeAlgorithm 
        {
            get 
            {
                if (this.session != null && this.session.IsAuthenticated)
                {
                    return this.session.CipherSuite.ExchangeAlgorithmType;
                }
                return ExchangeAlgorithmType.None;
            }
        }

        public virtual int KeyExchangeStrength 
        {
            get 
            {
                if (this.session != null && this.session.IsAuthenticated)
                {
                    return this.session.CipherSuite.KeyBlockSize;
                }
                return 0;
            }
        }

        public virtual X509Certificate LocalCertificate 
        {
            get 
            { 
                if (this.session != null && this.session.IsAuthenticated)
                {
                    if (!this.IsServer)
                    {
                        return this.session.ClientCertificate;
                    }
                    else
                    {
                        return this.session.LocalCertificates[0];
                    }
                }
                return null; 
            }
        }

        public virtual X509Certificate RemoteCertificate 
        {
            get 
            { 
                if (this.session != null && this.session.IsAuthenticated)
                {
                    return this.session.RemoteCertificates[0];
                }
                return null; 
            }
        }

        public virtual SslProtocols SslProtocol
        {
            get 
            {
                if (this.IsAuthenticated)
                {
                    return this.session.CurrentProtocolType;
                }
                return SslProtocols.None;
            }
        }

        public override int ReadTimeout 
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public override int WriteTimeout 
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        #endregion

        #region · Constructors ·

        public SslStream(Stream innerStream) : this(innerStream, true, null, null)
        {
        }

        public SslStream(Stream innerStream, bool leaveStreamOpen) : this(innerStream, leaveStreamOpen, null, null)
        {
        }

        public SslStream(
            Stream innerStream, 
            bool leaveStreamOpen,
            RemoteCertificateValidationCallback userCertificateValidationCallback)
            : this(innerStream, leaveStreamOpen, userCertificateValidationCallback, null)
        {
        }

        public SslStream(
            Stream innerStream, 
            bool leaveStreamOpen,
            RemoteCertificateValidationCallback userCertificateValidationCallback,
            LocalCertificateSelectionCallback userCertificateSelectionCallback)
            : base(innerStream, leaveStreamOpen)
        {
            if (innerStream == null)
            {
                throw new ArgumentNullException("stream is null.");
            }

            if (!innerStream.CanRead || !innerStream.CanWrite)
            {
                throw new ArgumentNullException("stream is not both readable and writable.");
            }

            this.userCertificateValidationCallback  = userCertificateValidationCallback;
            this.userCertificateSelectionCallback   = userCertificateSelectionCallback;

            this.innerStream    = innerStream;
            this.read           = new object();
            this.write          = new object();
            this.syncObject     = new object();
        }

        #endregion

        #region · Synchronous client authentication ·

        public virtual void AuthenticateAsClient(string targetHost)
        {
            this.AuthenticateAsClient(targetHost, null, SslProtocols.Tls, false);
        }

        public virtual void AuthenticateAsClient(
            string targetHost, X509CertificateCollection clientCertificates,
            SslProtocols sslProtocolType, bool checkCertificateRevocation)
        {
            // Note: Async code may have problem if they can't ensure that
            // the Negotiate phase isn't done during a read operation.
            // System.Net.HttpWebRequest protects itself from that problem
            lock (this.syncObject)
            {
                this.session = new SecureSession(this.innerStream);
                this.session.UserCertificateValidationCallback  = this.userCertificateValidationCallback;
                this.session.UserCertificateSelectionCallback   = this.userCertificateSelectionCallback;
                this.session.AuthenticateAsClient(
                    targetHost,
                    clientCertificates,
                    sslProtocolType,
                    checkCertificateRevocation);
            }
        }

        #endregion

        #region · Synchronous server authentication methods ·

        public virtual void AuthenticateAsServer(X509Certificate serverCertificate)
        {
            this.AuthenticateAsServer(serverCertificate, false, SslProtocols.Tls, false);
        }

        public virtual void AuthenticateAsServer(
            X509Certificate serverCertificate, 
            bool clientCertificateRequired,
            SslProtocols sslProtocolType, 
            bool checkCertificateRevocation)
        {
            // Note: Async code may have problem if they can't ensure that
            // the Negotiate phase isn't done during a read operation.
            // System.Net.HttpWebRequest protects itself from that problem
            lock (this.syncObject)
            {
                this.session = new SecureSession(this.innerStream);
                this.session.AuthenticateAsServer(
                    serverCertificate, 
                    clientCertificateRequired,
                    sslProtocolType,
                    checkCertificateRevocation);
            }
        }

        #endregion

        #region · Aysnchronous client authentication methods ·

        public virtual IAsyncResult BeginAuthenticateAsClient(string targetHost, AsyncCallback asyncCallback, object asyncState)
        {
            return this.BeginAuthenticateAsClient(
                targetHost,
                null,
                SslProtocols.Tls,
                true,
                asyncCallback,
                asyncState);
        }

        public virtual IAsyncResult BeginAuthenticateAsClient(string targetHost, X509CertificateCollection clientCertificates,
            SslProtocols sslProtocolType, bool checkCertificateRevocation,
            AsyncCallback asyncCallback, object asyncState)
        {
            if (targetHost == null || targetHost.Length == 0)
            {
                throw new ArgumentNullException("targetHost is null or an empty string.");
            }

            throw new NotSupportedException();
        }

        public virtual void EndAuthenticateAsClient(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult is null or was not obtained by calling BeginRead.");
            }

            throw new NotSupportedException();
        }

        #endregion

        #region · Asynchronous server authentication methods ·

        public virtual IAsyncResult BeginAuthenticateAsServer(
            X509Certificate serverCertificate, AsyncCallback asyncCallback,
            object asyncState)
        {
            return this.BeginAuthenticateAsServer(serverCertificate, false, SslProtocols.Tls, true, asyncCallback, asyncState);
        }

        public virtual IAsyncResult BeginAuthenticateAsServer(
            X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols sslProtocolType,
            bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState)
        {
            throw new NotSupportedException();
        }

        public virtual void EndAuthenticateAsServer(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
                throw new ArgumentNullException("asyncResult is null or was not obtained by calling BeginRead.");

            throw new NotSupportedException();
        }

        #endregion

        #region · Asynchronous Read/Write Methods ·

        public override IAsyncResult BeginRead(
            byte[]			buffer,
            int				offset,
            int				count,
            AsyncCallback	callback,
            object			state)
        {
            this.CheckDisposed();
            
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer is a null reference.");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset is less than 0.");
            }
            if (offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("offset is greater than the length of buffer.");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count is less than 0.");
            }
            if (count > (buffer.Length - offset))
            {
                throw new ArgumentOutOfRangeException("count is less than the length of buffer minus the value of the offset parameter.");
            }

            IAsyncResult asyncResult = null;

            lock (this.read)
            {
                try
                {
                    /*
                    // If actual buffer is full readed reset it
                    if (this.inputBuffer.Position == this.inputBuffer.Length &&
                        this.inputBuffer.Length > 0)
                    {
                        this.ResetBuffer();
                    }

                    if (this.IsAuthenticated)
                    {
                        if ((this.inputBuffer.Length == this.inputBuffer.Position) && (count > 0))
                        {
                            // bigger than max record length for SSL/TLS
                            byte[] recbuf = new byte[16384]; 

                            // this will read data from the network until we have (at least) one
                            // record to send back to the caller
                            this.innerStream.BeginRead(recbuf, 0, recbuf.Length, new AsyncCallback(NetworkReadCallback), recbuf);

                            if (!recordEvent.WaitOne(300000, false)) // 5 minutes
                            {
                                // FAILSAFE : AlertDescription.InternalError
                                throw new SecureException ("Internal error");
                            }
                        }
                    }

                    // return the record(s) to the caller
                    asyncResult = rd.BeginInvoke(buffer, offset, count, callback, state);
                    */
                }
                catch (SecureException)
                {
                    // this.protocol.SendAlert(ex.Alert);
                    this.Close();

                    throw new IOException("The authentication or decryption has failed.");
                }
                catch (Exception)
                {
                    throw new IOException("IO exception during read.");
                }

            }

            return asyncResult;
        }

        public override IAsyncResult BeginWrite(
            byte[]			buffer,
            int				offset,
            int				count,
            AsyncCallback	callback,
            object			state)
        {
            this.CheckDisposed();

            if (buffer == null)
            {
                throw new ArgumentNullException("buffer is a null reference.");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset is less than 0.");
            }
            if (offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("offset is greater than the length of buffer.");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count is less than 0.");
            }
            if (count > (buffer.Length - offset))
            {
                throw new ArgumentOutOfRangeException("count is less than the length of buffer minus the value of the offset parameter.");
            }

            IAsyncResult asyncResult;

            lock (this.write)
            {
                try
                {		
                    asyncResult = this.session.BeginWrite(
                        buffer, offset, count, callback, state);
                }
                catch (SecureException)
                {
                    // this.protocol.SendAlert(ex.Alert);
                    this.Close();

                    throw new IOException("The authentication or decryption has failed.");
                }
                catch (Exception)
                {
                    throw new IOException("IO exception during Write.");
                }
            }

            return asyncResult;
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            this.CheckDisposed();

            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult is null or was not obtained by calling BeginRead.");
            }

            return this.InnerStream.EndRead(asyncResult);
        }       

        public override void EndWrite(IAsyncResult asyncResult)
        {
            this.CheckDisposed();

            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult is null or was not obtained by calling BeginRead.");
            }

            this.InnerStream.EndWrite(asyncResult);
        }

        #endregion

        #region · Methods ·

        public override void Close()
        {
            ((IDisposable)this).Dispose();
        }

        public override void Flush()
        {
            this.CheckDisposed();

            this.innerStream.Flush();
        }

        public int Read(byte[] buffer)
        {
            return this.Read(buffer, 0, buffer.Length);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            IAsyncResult res = this.BeginRead(buffer, offset, count, null, null);

            return this.EndRead(res);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }
        
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public void Write(byte[] buffer)
        {
            this.Write(buffer, 0, buffer.Length);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            IAsyncResult res = this.BeginWrite (buffer, offset, count, null, null);

            this.EndWrite(res);
        }

        #endregion

        #region · Misc Methods ·

        private void ResetBuffer()
        {
            this.inputBuffer.SetLength(0);
            this.inputBuffer.Position = 0;
        }

        private void CheckDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("The SslClientStream is closed.");
            }
        }

        #endregion
    }
}

#endif

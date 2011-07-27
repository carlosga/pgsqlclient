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
using System.Security.Cryptography;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Mono.Security.Cryptography;
using X509 = Mono.Security.X509;

namespace SecureSocketLayer.Net.Security.Providers.Common
{
	internal class SecureSession
	{
		#region · Callback Fields ·

        private RemoteCertificateValidationCallback userCertificateValidationCallback;
        private LocalCertificateSelectionCallback userCertificateSelectionCallback;

		#endregion

		#region · Callback Properties ·

        public RemoteCertificateValidationCallback UserCertificateValidationCallback
		{
            get { return this.userCertificateValidationCallback; }
            set { this.userCertificateValidationCallback = value; }
		}

        public LocalCertificateSelectionCallback UserCertificateSelectionCallback
		{
			get { return this.userCertificateSelectionCallback; }
			set { this.userCertificateSelectionCallback = value; }
		}

		#endregion

        #region · Fields ·

		private string				targetHost;
		private bool				isAuthenticated;
		private bool				isEncrypted;
		private bool				isMutuallyAuthenticated;
		private bool				isServer;
		private bool				isClosed;
		private bool				clientCertificateRequired;
		private bool				checkCertificateRevocation;
		private SslProtocols		initialProtocolType;
		private SslProtocols		currentProtocolType;
		private CipherSuite			cipherSuite;
		private ISecureAuthenticator authenticator;
		private ISecureFactory		secureFactory;
		private ISecureProtocol		secureProtocol;
		private ISecureKeyInfo		secureKeyInfo;
		private IRecordEncryptor	recordEncryptor;
		private IRecordDecryptor	recordDecryptor;
		private IRecordMACManager	macManager;
		private Stream				inputStream;
		private Stream				outputStream;
		private X509CertificateCollection remoteCertificates;
		private X509CertificateCollection localCertificates;
		private X509Certificate	    clientCertificate;
        private X509Chain           remoteCertificateChain;
		private RSAParameters		rsaParameters;
		private MemoryStreamEx		handshakeMessages;

		#endregion

		#region · Properties ·

		public string TargetHost 
		{ 
			get { return this.targetHost; }
			set { this.targetHost = value; }
		}
		
		public SslProtocols InitialProtocolType
		{ 
			get { return this.initialProtocolType; }
			set { this.initialProtocolType = value; }
		}

		public SslProtocols CurrentProtocolType
		{ 
			get { return this.currentProtocolType; }
			set { this.currentProtocolType = value; }
		}

		public bool IsAuthenticated 
		{
			get { return this.isAuthenticated; }
		}

		public bool IsEncrypted 
		{
			get { return this.isEncrypted; }
		}

		public bool IsMutuallyAuthenticated 
		{
			get { return this.isMutuallyAuthenticated; }
		}

		public bool	IsServer
		{
			get { return this.isServer; }
		}

		public bool	IsSigned
		{
			get { return this.CipherSuite.ExchangeAlgorithmType == ExchangeAlgorithmType.RsaSign; }
		}

		public bool	ClientCertificateRequired
		{
			get { return this.clientCertificateRequired; }
		}

		public bool CheckCertificateRevocation
		{
			get { return this.checkCertificateRevocation; }
		}

		public CipherSuiteCollection SupportedCipherSuites
		{
			get { return this.secureFactory.SupportedCipherSuites; }
		}

		public CompressionMethodCollection SupportedCompressionMethods
		{
			get { return this.secureFactory.SupportedCompressionMethods; }
		}

		public CipherSuite CipherSuite
		{
			get { return this.cipherSuite; }
		}

		public X509CertificateCollection RemoteCertificates
		{
			get
			{
				if (this.remoteCertificates == null)
				{
					this.remoteCertificates = new X509CertificateCollection();
				}

				return this.remoteCertificates;
			}
		}

		public X509CertificateCollection LocalCertificates
		{
			get
			{
				if (this.localCertificates	== null)
				{
					this.localCertificates = new X509CertificateCollection();
				}

				return this.localCertificates;
			}
		}

		public X509Certificate ClientCertificate
		{
			get { return this.clientCertificate; }
		}

		#endregion

		#region · Internal Properties ·

		internal ISecureProtocol SecureProtocol
		{
			get { return this.secureProtocol; }
		}

		internal ISecureKeyInfo SecureKeyInfo
		{
			get { return this.secureKeyInfo; }
		}

		internal IRecordEncryptor Encryptor
		{
			get { return this.recordEncryptor; }
		}

		internal IRecordDecryptor Decryptor
		{
			get { return this.recordDecryptor; }
		}

		internal IRecordMACManager MACManager
		{
			get { return this.macManager; }
		}

		internal byte[] HandshakeMessages
		{
			get { return this.handshakeMessages.ToArray(); }
		}

		internal Stream InputStream
		{
			get { return this.inputStream; }
			set 
			{ 
				this.inputStream = value; 
				if (this.secureProtocol != null)
				{
					this.secureProtocol.InputStream = value;
				}
			}
		}

		internal Stream OutputStream
		{
			get { return this.outputStream; }
			set 
			{ 
				this.outputStream = value; 
				if (this.secureProtocol != null)
				{
					this.secureProtocol.OutputStream = value;
				}
			}
		}

		#endregion

		#region · Constructors ·

		public SecureSession(Stream innerStream)
		{
			this.inputStream	= innerStream;
			this.outputStream	= innerStream;
		}

		public SecureSession(Stream inputStream, Stream outputStream)
		{
			this.inputStream	= inputStream;
			this.outputStream	= outputStream;
		}

		#endregion

		#region · Synchronous client authentication ·

		public void AuthenticateAsClient(string targetHost)
		{
			this.AuthenticateAsClient(targetHost, null, SslProtocols.Tls, false);
		}

		public void AuthenticateAsClient(
			string targetHost, 
			X509CertificateCollection clientCertificates,
			SslProtocols sslProtocolType, 
			bool checkCertificateRevocation)
		{
			this.targetHost					= targetHost;
			this.checkCertificateRevocation = checkCertificateRevocation;
			this.initialProtocolType		= sslProtocolType;

			// Set the current protocol type (this will be used on Client Hello)
			if ((this.initialProtocolType & SslProtocols.Ssl3) == SslProtocols.Ssl3) 
			{
				this.currentProtocolType = SslProtocols.Ssl3;
			}
			else
			{
				this.currentProtocolType = SslProtocols.Tls;
			}

			// Initialize session
			this.Initialize(this.currentProtocolType);

			// Local client certificates as local certs
			if (clientCertificates != null)
			{
				this.LocalCertificates.AddRange(clientCertificates);
			}

			// Initialize key information
			this.secureKeyInfo = this.secureFactory.CreateKeyInfo();

			// Initialize handshake messages stream
			this.handshakeMessages = new MemoryStreamEx();

			this.Authenticate();
		}

		#endregion

		#region · Synchronous Server authentication ·

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
			throw new NotImplementedException();
		}

		#endregion

		#region · Internal Methods ·

		internal short GetClientHelloProtocol()
		{
			if ((this.initialProtocolType & SslProtocols.Ssl3) == SslProtocols.Ssl3) 
			{
				return Helper.GetProtocolCode(SslProtocols.Ssl3);
			}
			else
			{
				return Helper.GetProtocolCode(SslProtocols.Tls);
			}
		}

		internal RSA GetRSAExchangeAlgortihm()
		{
			RSA rsa = null;

			if (this.CipherSuite.IsExportable) 
			{
				// this is the case for "exportable" ciphers
				rsa = new RSAManaged();
				rsa.ImportParameters(this.rsaParameters);
			}
			else 
			{
				rsa = this.GetServerCertificateRSA();
			}

			return rsa;
		}

		internal RSA GetServerCertificateRSA()
		{
			RSA rsa = null;

			X509.X509Certificate cert = new X509.X509Certificate(this.remoteCertificates[0].GetRawCertData());

			rsa = new RSAManaged(cert.RSA.KeySize);
			rsa.ImportParameters(cert.RSA.ExportParameters(false));			

			return rsa;
		}

		#endregion

		#region · Synchronous Receive/Send Methods ·

		public SecureRecord Read()
		{
			if (this.isClosed)
			{
				throw new InvalidOperationException("The session is Closed.");
			}
			return this.SecureProtocol.Read();
		}
		
		public void Write(byte[] buffer)
		{
			if (this.isClosed)
			{
				throw new InvalidOperationException("The session is Closed.");
			}
			this.SecureProtocol.Write(buffer);
		}

		internal void Write(ContentType contentType, byte[] buffer)
		{
			if (this.isClosed)
			{
				throw new InvalidOperationException("The session is Closed.");
			}
			this.SecureProtocol.Write(contentType, buffer);			
		}

		public IAsyncResult BeginRead(
			byte[]			buffer,
			int				offset,
			int				count,
			AsyncCallback	callback,
			object			state)
		{
			if (this.isClosed)
			{
				throw new InvalidOperationException("The session is Closed.");
			}
			return this.SecureProtocol.BeginRead(buffer, offset, count, callback, state);			
		}

		public int EndRead(IAsyncResult asyncResult)
		{
			if (this.isClosed)
			{
				throw new InvalidOperationException("The session is Closed.");
			}
			return this.SecureProtocol.EndRead(asyncResult);
		}

		public IAsyncResult BeginWrite(
			byte[]			buffer,
			int				offset,
			int				count,
			AsyncCallback	callback,
			object			state)
		{
			if (this.isClosed)
			{
				throw new InvalidOperationException("The session is Closed.");
			}
			return this.SecureProtocol.BeginWrite(buffer, offset, count, callback, state);
		}
		
		public void EndWrite(IAsyncResult asyncResult)
		{
			if (this.isClosed)
			{
				throw new InvalidOperationException("The session is Closed.");
			}
			this.SecureProtocol.EndWrite(asyncResult);
		}

		#endregion

		#region · Methods ·

		public void Close()
		{
			this.isClosed					= true;
			this.isAuthenticated			= false;
			this.isMutuallyAuthenticated	= false;
			this.isServer					= false;
			this.isEncrypted				= false;
			this.authenticator				= null;
			this.secureKeyInfo				= null;
			this.secureFactory				= null;
			this.secureProtocol				= null;
		}

		#endregion

		#region · Authenticator Callbacks Handlers ·

		private void ProtocolChanged(short protocol)
		{
			this.currentProtocolType = Helper.GetProtocolType(protocol);
			this.Initialize(this.currentProtocolType);
			this.authenticator.MessageGenerator = this.secureFactory.CreateHandshakeMessageGenerator(this.authenticator);
			this.authenticator.MessageProcessor = this.secureFactory.CreateHandshakeMessageProcessor(this.authenticator);
		}

		private void GeneratedClientRandom(byte[] random)
		{
			this.SecureKeyInfo.ClientRandom = random;
		}

		private void ReceivedServerRandom(byte[] random)
		{
			this.SecureKeyInfo.ServerRandom = random;
		}

		private void CipherSuiteSelected(short cipherSuite)
		{
			this.cipherSuite = this.secureFactory.SupportedCipherSuites[cipherSuite];
		}

		private void CompressionMethodSelected(byte compressionMethod)
		{
#warning FIXME !!! Handle Compression
		}

		private void RemoteCertificateReceived(X509Certificate certificate)
		{
            if (this.remoteCertificateChain == null)
            {
                this.remoteCertificateChain = new X509Chain();

                try
                {
                    this.remoteCertificateChain.Build(new X509Certificate2(certificate));
                }
                catch
                {
                    this.remoteCertificateChain.Reset();
                }
            }

			this.RemoteCertificates.Add(certificate);
		}

		private void ReceivedRemoteCertificateChain()
		{
            SslPolicyErrors errors = SslPolicyErrors.None;

            if (this.remoteCertificateChain.ChainElements.Count > 0)
            {
                IX509ChainValidator validator = this.secureFactory.CreateX509ChainValidator(
                    this.remoteCertificateChain,
                    this.CipherSuite.ExchangeAlgorithmType);

                errors |= validator.Validate(this.RemoteCertificates, this.TargetHost);
            }
            else
            {
                errors |= SslPolicyErrors.RemoteCertificateChainErrors;
            }

            if (this.UserCertificateValidationCallback != null)
            {
                if (!this.UserCertificateValidationCallback(this, this.RemoteCertificates[0], this.remoteCertificateChain, errors))
                {
                    throw new SecureException("The remote certificate is invalid according to the validation procedure.");
                }
            }
            else
            {
                if (errors != SslPolicyErrors.None)
                {
                    throw new SecureException("The remote certificate is invalid according to the validation procedure.");
                }
            }
		}

		private void ClientCertificateSelected(X509Certificate certificate)
		{	
			this.clientCertificate = this.UserCertificateSelectionCallback(
				this, this.TargetHost, this.LocalCertificates, this.RemoteCertificates[0], null);

			if (this.clientCertificate == null)
			{
				// AlertDescription.UserCancelled
				throw new SecureException("Invalid Client certificate.");
			}
		}

		private void ReceivedServerKeyExchange(RSAParameters rsaParameters)
		{
			this.rsaParameters = rsaParameters;
		}

		private void ClientCertificateRequested(ClientCertificateType[] certificateTypes, string[] distinguisedNames)
		{
			this.clientCertificateRequired	= true;
			this.isMutuallyAuthenticated	= true;
		}

		private void ExchangingKeys()
		{
			ISecureKeyGenerator secureKeyGenerator = this.secureFactory.CreateKeyGenerator(this);
			secureKeyGenerator.Generate(ref this.secureKeyInfo);
		}

		private void ExchangedKeys()
		{
		}

		private void ChangedCipherSpec()
		{
			this.isEncrypted		= true;
			this.macManager			= this.secureFactory.CreateMACManager(this);
			this.recordEncryptor	= this.secureFactory.CreateRecordEncryptor(this);
			this.recordDecryptor	= this.secureFactory.CreateRecordDecryptor(this);
		}

		private void AuthenticationFinished()
		{
			this.isAuthenticated	= true;
			this.authenticator		= null;
			this.secureKeyInfo		= null;
			
			this.handshakeMessages.Close();
			this.handshakeMessages	= null;
		}

		#endregion

		#region · SecureProtocol Callbacks ·

		private void RecordReceived(SecureRecord record)
		{
			if (record.ContentType == ContentType.Handshake)
			{
				this.handshakeMessages.Write(record.Fragment);
			}
		}

		private void RecordSent(SecureRecord record)
		{
			if (record.ContentType == ContentType.Handshake)
			{
				this.handshakeMessages.Write(record.Fragment);
			}
		}

		#endregion

		#region · Private Methods ·

		private void Authenticate()
		{
			// Perform the authentication
			this.authenticator	= this.secureFactory.CreateSecureAuthenticator(this);

			// Configure Authenticator Callbacks
			this.authenticator.ProtocolChanged				= new ProtocolChangedCallback(ProtocolChanged);
			this.authenticator.GeneratedClientRandom		= new GeneratedClientRandomCallback(GeneratedClientRandom);
			this.authenticator.CipherSuiteSelected			= new CipherSuiteSelectedCallback(CipherSuiteSelected);
			this.authenticator.CompressionMethodSelected	= new CompressionMethodSelectedCallback(CompressionMethodSelected);
			this.authenticator.ReceivedServerRandom			= new ReceivedServerRandomCallback(ReceivedServerRandom);
			this.authenticator.ReceivedServerKeyExchange	= new ReceivedServerKeyExchangeCallback(ReceivedServerKeyExchange);
			this.authenticator.RemoteCertificateReceived	= new RemoteCertificateReceivedCallback(RemoteCertificateReceived);
			this.authenticator.ReceivedRemoteCertificateChain = new ReceivedRemoteCertificateChainCallback(ReceivedRemoteCertificateChain);
			this.authenticator.ClientCertificateSelected	= new ClientCertificateSelectedCallback(ClientCertificateSelected);
			this.authenticator.ClientCertificateRequested	= new ClientCertificateRequestedCallback(ClientCertificateRequested);
			this.authenticator.ExchangingKeys				= new ExchangingKeysCallback(ExchangingKeys);
			this.authenticator.ExchangedKeys				= new ExchangedKeysCallback(ExchangedKeys);			
			this.authenticator.ChangedCipherSpec			= new ChangedCipherSpecCallback(ChangedCipherSpec);
			this.authenticator.AuthenticationFinished		= new AuthenticationFinishedCallback(AuthenticationFinished);

			// Perform Authentication
			this.authenticator.Authenticate();
		}

		private void Initialize(SslProtocols protocol)
		{
			this.isClosed			= false;
			this.secureFactory		= this.GetFactory(this.currentProtocolType, true);
			this.secureProtocol		= this.secureFactory.CreateSecureProtocol(this);

			// Configure input and output streams used by the secure protocol
			this.secureProtocol.InputStream		= this.inputStream;
			this.secureProtocol.OutputStream	= this.outputStream;

			// Configure Secure Protocol Callbacks handlers
			this.secureProtocol.RecordReceived	= new RecordReceivedCallback(RecordReceived);
			this.secureProtocol.RecordSent		= new RecordSentCallback(RecordSent);
		}

		private ISecureFactory GetFactory(SslProtocols protocol, bool isClient)
		{
			if (isClient)
			{
				switch (protocol)
				{
					case SslProtocols.Ssl3:
						return Ssl.Client.SslClientFactory.Instance;

					case SslProtocols.Tls:
						return Tls.Client.TlsClientFactory.Instance;

					default:
						throw new NotSupportedException();
				}
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		#endregion
	}
}

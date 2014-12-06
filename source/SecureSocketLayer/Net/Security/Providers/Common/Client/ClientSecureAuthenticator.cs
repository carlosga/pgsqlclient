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

namespace SecureSocketLayer.Net.Security.Providers.Common.Client
{
    internal sealed class ClientSecureAuthenticator : ISecureAuthenticator
    {
        #region · Callbacks Fields ·

        private GeneratedClientRandomCallback generatedClientRandom;
        private ReceivedServerRandomCallback receivedServerRandom;
        private ProtocolChangedCallback protocolChanged;
        private CipherSuiteSelectedCallback cipherSuiteSelected;
        private CompressionMethodSelectedCallback compressionMethodSelected;
        private RemoteCertificateReceivedCallback remoteCertificateReceived;
        private ReceivedRemoteCertificateChainCallback remoteCertificateChainReceived;
        private ClientCertificateSelectedCallback clientCertificateSelected;
        private ReceivedServerKeyExchangeCallback receivedServerKeyExchange;
        private ExchangingKeysCallback exchangingKeys;
        private ExchangedKeysCallback exchangedKeys;
        private ChangedCipherSpecCallback changedCipherSpec;
        private ClientCertificateRequestedCallback clientCertificateRequested;
        private AuthenticationFinishedCallback authenticationFinished;

        #endregion

        #region · Callbacks Properties ·

        public GeneratedClientRandomCallback GeneratedClientRandom 
        { 
            get { return this.generatedClientRandom; }
            set { this.generatedClientRandom = value; }  
        }

        public ReceivedServerRandomCallback ReceivedServerRandom 
        { 
            get { return this.receivedServerRandom; }
            set { this.receivedServerRandom = value; }
        }

        public ProtocolChangedCallback ProtocolChanged 
        { 
            get { return this.protocolChanged; }
            set { this.protocolChanged = value; }
        }

        public CipherSuiteSelectedCallback CipherSuiteSelected 
        { 
            get { return this.cipherSuiteSelected; }
            set { this.cipherSuiteSelected = value; } 
        }

        public CompressionMethodSelectedCallback CompressionMethodSelected 
        { 
            get { return this.compressionMethodSelected; } 
            set { this.compressionMethodSelected = value; } 
        }

        public RemoteCertificateReceivedCallback RemoteCertificateReceived 
        { 
            get { return this.remoteCertificateReceived; }
            set { this.remoteCertificateReceived = value; }
        }

        public ReceivedRemoteCertificateChainCallback ReceivedRemoteCertificateChain
        { 
            get { return this.remoteCertificateChainReceived; }
            set { this.remoteCertificateChainReceived = value; }
        }

        public ClientCertificateSelectedCallback ClientCertificateSelected 
        { 
            get { return this.clientCertificateSelected; }
            set { this.clientCertificateSelected = value; } 
        }

        public ReceivedServerKeyExchangeCallback ReceivedServerKeyExchange
        {
            get { return this.receivedServerKeyExchange; }
            set { this.receivedServerKeyExchange = value; }
        }

        public ExchangingKeysCallback ExchangingKeys
        {
            get { return this.exchangingKeys; }
            set { this.exchangingKeys = value; }
        }

        public ExchangedKeysCallback ExchangedKeys
        {
            get { return this.exchangedKeys; }
            set { this.exchangedKeys = value; }
        }

        public ChangedCipherSpecCallback ChangedCipherSpec 
        { 
            get { return this.changedCipherSpec; }
            set { this.changedCipherSpec = value; }
        }

        public ClientCertificateRequestedCallback ClientCertificateRequested 
        { 
            get { return this.clientCertificateRequested; }
            set { this.clientCertificateRequested = value; }
        }

        public AuthenticationFinishedCallback AuthenticationFinished 
        { 
            get { return this.authenticationFinished; }
            set { this.authenticationFinished = value; } 
        }

        #endregion

        #region · Fields ·

        private SecureSession				session;
        private IHandshakeMessageProcessor	messageProcessor;
        private IHandshakeMessageGenerator	messageGenerator;

        #endregion

        #region · Properties ·

        public SecureSession SecureSession
        {
            get { return this.session; } 
        }

        public IHandshakeMessageGenerator MessageGenerator
        {
            get { return this.messageGenerator; }
            set { this.messageGenerator = value; }
        }

        public IHandshakeMessageProcessor MessageProcessor
        {
            get { return this.messageProcessor; }
            set { this.messageProcessor = value; }
        }

        #endregion

        #region · Protected Constructors ·

        public ClientSecureAuthenticator(SecureSession session)
        {
            this.session = session;
        }

        #endregion

        #region · IAuthenticator Methods ·

        /*
            Client											Server

            ClientHello                 -------->
                                                            ServerHello
                                                            Certificate*
                                                            ServerKeyExchange*
                                                            CertificateRequest*
                                        <--------			ServerHelloDone
            Certificate*
            ClientKeyExchange
            CertificateVerify*
            [ChangeCipherSpec]
            Finished                    -------->
                                                            [ChangeCipherSpec]
                                        <--------           Finished
            Application Data            <------->			Application Data

                    Fig. 1 - Message flow for a full handshake		
        */

        public void Authenticate()
        {
            // Handshake negotiation
            SecureSession session = this.SecureSession;

            try
            {
                // Send client hello
                this.Send(HandshakeMessageType.ClientHello);

                // Read server response
                HandshakeMessageType lastMessage = HandshakeMessageType.None;
                while (lastMessage != HandshakeMessageType.ServerHelloDone)
                {
                    lastMessage = this.ProcessRecord(session.Read());
                }

                // Send client certificate if requested
                if (this.SecureSession.ClientCertificateRequired)
                {
                    this.Send(HandshakeMessageType.Certificate);
                }

                // Send Client Key Exchange
                this.Send(HandshakeMessageType.ClientKeyExchange);

                // Send certificate verify if requested
                if (this.SecureSession.ClientCertificateRequired)
                {
                    this.Send(HandshakeMessageType.CertificateVerify);
                }

                // Send Cipher Spec protocol
                session.Write(ContentType.ChangeCipherSpec, new byte[] { 1 });
                if (this.ChangedCipherSpec != null)
                {
                    this.ChangedCipherSpec();
                }

                // Send Finished Message
                this.Send(HandshakeMessageType.Finished);
            
                // Read record until server finished is received
                lastMessage = HandshakeMessageType.None;
                while (lastMessage != HandshakeMessageType.Finished)
                {
                    lastMessage = this.ProcessRecord(session.Read());
                }

                if (this.AuthenticationFinished != null)
                {
                    this.AuthenticationFinished();
                }
            }
            catch
            {
                throw new Exception("The authentication or decryption has failed.");
            }
        }

        #endregion

        #region · Private Methods ·

        private void Send(HandshakeMessageType messageType)
        {
            byte[] message = this.GenerateRecord(messageType);
            this.SecureSession.Write(ContentType.Handshake, message);
        }

        private byte[] GenerateRecord(HandshakeMessageType messageType)
        {
            switch (messageType)
            {
                case HandshakeMessageType.ClientHello:
                    return messageGenerator.ClientHello();

                case HandshakeMessageType.CertificateRequest:
                    return messageGenerator.CertificateRequest();

                case HandshakeMessageType.ClientKeyExchange:
                    return messageGenerator.ClientKeyExchange();

                case HandshakeMessageType.Certificate:
                    return messageGenerator.Certificate();

                case HandshakeMessageType.CertificateVerify:
                    return messageGenerator.CertificateVerify();

                case HandshakeMessageType.Finished:
                    return messageGenerator.Finished();
            }

            return null;
        }

        private HandshakeMessageType ProcessRecord(SecureRecord record)
        {
            HandshakeMessageType lastMessage = HandshakeMessageType.None;
            MemoryStreamEx messages = new MemoryStreamEx(record.Fragment);

            while (!messages.EOF)
            {
                lastMessage = (HandshakeMessageType)messages.ReadByte();
                byte[] buffer = messages.ReadBytes(messages.ReadInt24());

                switch (lastMessage)
                {
                    case HandshakeMessageType.ServerHello:
                        messageProcessor.ServerHello(buffer);
                        break;

                    case HandshakeMessageType.Certificate:
                        messageProcessor.Certificate(buffer);
                        break;

                    case HandshakeMessageType.ServerKeyExchange:
                        messageProcessor.ServerKeyExchange(buffer);
                        break;

                    case HandshakeMessageType.CertificateRequest:
                        messageProcessor.CertificateRequest(buffer);
                        break;

                    case HandshakeMessageType.Finished:
                        messageProcessor.Finished(buffer);
                        break;
                }
            }

            return lastMessage;
        }

        #endregion
    }
}

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

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SecureSocketLayer.Net.Security.Providers.Common
{
	internal delegate void GeneratedClientRandomCallback(byte[] random);
	internal delegate void ReceivedServerRandomCallback(byte[] random);
	internal delegate void ProtocolChangedCallback(short protocol);
	internal delegate void CipherSuiteSelectedCallback(short cipherSuite);
	internal delegate void CompressionMethodSelectedCallback(byte compressionMethod);
	internal delegate void ReceivedServerKeyExchangeCallback(RSAParameters rsaParameters);
	internal delegate void RemoteCertificateReceivedCallback(X509Certificate certificate);
	internal delegate void ReceivedRemoteCertificateChainCallback();
	internal delegate void ClientCertificateSelectedCallback(X509Certificate certificate);
	internal delegate void ClientCertificateRequestedCallback(ClientCertificateType[] certificateType, string[] distinguisedNames);
	internal delegate void ExchangingKeysCallback();
	internal delegate void ExchangedKeysCallback();
	internal delegate void ChangedCipherSpecCallback();
	internal delegate void AuthenticationFinishedCallback();
}

#endif
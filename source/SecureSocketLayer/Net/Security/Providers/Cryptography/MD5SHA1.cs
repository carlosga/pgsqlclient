// Secure Sockets Layer / Transport Security Layer Implementation
// Copyright(c) 2003-2005 Carlos Guzman Alvarez

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
using System.Security.Cryptography;

namespace SecureSocketLayer.Net.Security.Providers.Cryptography
{
    internal class MD5SHA1 : HashAlgorithm
    {
        #region � Fields �

        private HashAlgorithm md5;
        private HashAlgorithm sha;
        private bool hashing;

        #endregion

        #region � Constructors �

        public MD5SHA1() : base()
        {
            this.md5 = MD5.Create();
            this.sha = SHA1.Create();

            // Set HashSizeValue
            this.HashSizeValue = this.md5.HashSize + this.sha.HashSize;
        }

        #endregion

        #region � Methods �

        public override void Initialize()
        {
            this.md5.Initialize();
            this.sha.Initialize();
            this.hashing = false;
        }

        protected override byte[] HashFinal()
        {
            if (!hashing) 
            {
                this.hashing = true;
            }

            // Finalize the original hash
            this.md5.TransformFinalBlock(new byte[0], 0, 0);
            this.sha.TransformFinalBlock(new byte[0], 0, 0);

            byte[] hash = new byte[36];

            Buffer.BlockCopy(this.md5.Hash, 0, hash, 0, 16);
            Buffer.BlockCopy(this.sha.Hash, 0, hash, 16, 20);

            return hash;
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            if (!hashing) 
            {
                hashing = true;
            }

            this.md5.TransformBlock(array, ibStart, cbSize, array, ibStart);
            this.sha.TransformBlock(array, ibStart, cbSize, array, ibStart);
        }

        public byte[] CreateSignature(AsymmetricAlgorithm key)
        {
            if (key == null) 
            {
                throw new CryptographicUnexpectedOperationException("missing key");
            }

            RSASslSignatureFormatter f = new RSASslSignatureFormatter(key);
            f.SetHashAlgorithm("MD5SHA1");

            return f.CreateSignature(this.Hash);
        }

        public bool VerifySignature(AsymmetricAlgorithm key, byte[] rgbSignature)
        {
            if (key == null) 
            {
                throw new CryptographicUnexpectedOperationException("missing key");
            }
            if (rgbSignature == null) 
            {
                throw new ArgumentNullException("rgbSignature");
            }

            RSASslSignatureDeformatter d = new RSASslSignatureDeformatter(key);
            d.SetHashAlgorithm("MD5SHA1");

            return d.VerifySignature(this.Hash, rgbSignature);
        }

        #endregion
    }
}

#endif
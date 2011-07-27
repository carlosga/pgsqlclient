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
    /*
     * References:
     * 		RFC 2104(http://www.ietf.org/rfc/rfc2104.txt)
     *		RFC 2202(http://www.ietf.org/rfc/rfc2202.txt)
     * MSDN:
     * 
     *		Extending the KeyedHashAlgorithm Class(http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpguide/html/cpconextendingkeyedhashalgorithmclass.asp)
     */
    internal class HMACSsl : System.Security.Cryptography.KeyedHashAlgorithm
    {
        #region · Fields ·

        private HashAlgorithm hash;
        private bool hashing;
        private byte[] innerPad;
        private byte[] outerPad;

        #endregion

        #region · Properties ·

        public override byte[] Key
        {
            get { return (byte[])base.KeyValue.Clone(); }
            set
            {
                if (this.hashing) 
                {
                    throw new Exception("Cannot change key during hash operation.");
                }

                /* if key is longer than 64 bytes reset it to rgbKey = Hash(rgbKey) */
                if (value.Length > 64) 
                {
                    base.KeyValue = this.hash.ComputeHash(value);
                }
                else 
                {
                    base.KeyValue = (byte[])value.Clone();
                }

                this.InitializePad();
            }
        }

        #endregion

        #region · Constructors ·

        public HMACSsl()
        {
            // Create the hash
            this.hash = MD5.Create();

            // Set HashSizeValue
            base.HashSizeValue = this.hash.HashSize;

            // Generate a radom key
            byte[] rgbKey = new byte[64];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetNonZeroBytes(rgbKey);

            base.KeyValue = (byte[])rgbKey.Clone();

            this.Initialize();
        }

        public HMACSsl(string hashName, byte[] rgbKey)
        {
            // Create the hash
            if (hashName == null || hashName.Length == 0) 
            {
                hashName = "MD5";
            }
            this.hash = HashAlgorithm.Create(hashName);

            // Set HashSizeValue
            base.HashSizeValue = this.hash.HashSize;

            /* if key is longer than 64 bytes reset it to rgbKey = Hash(rgbKey) */
            if (rgbKey.Length > 64) 
            {
                base.KeyValue = this.hash.ComputeHash(rgbKey);
            }
            else 
            {
                base.KeyValue = (byte[])rgbKey.Clone();
            }

            this.Initialize();
        }

        #endregion

        #region · Methods ·

        public override void Initialize()
        {
            this.hash.Initialize();
            this.InitializePad();
            this.hashing = false;
        }

        protected override byte[] HashFinal()
        {
            if (!this.hashing) 
            {
                this.hash.TransformBlock(this.innerPad, 0, this.innerPad.Length, this.innerPad, 0);
                this.hashing = true;
            }

            // Finalize the original hash
            this.hash.TransformFinalBlock(new byte[0], 0, 0);

            byte[] firstResult = this.hash.Hash;

            this.hash.Initialize();
            this.hash.TransformBlock(this.outerPad, 0, this.outerPad.Length, this.outerPad, 0);
            this.hash.TransformFinalBlock(firstResult, 0, firstResult.Length);

            this.Initialize();

            return this.hash.Hash;
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            if (!this.hashing) 
            {
                this.hash.TransformBlock(this.innerPad, 0, this.innerPad.Length, this.innerPad, 0);
                this.hashing = true;
            }

            this.hash.TransformBlock(array, ibStart, cbSize, array, ibStart);
        }

        #endregion

        #region · Private Methods ·

        private void InitializePad()
        {
            // Fill pad arrays
            innerPad = new byte[64];
            outerPad = new byte[64];

            /* Pad the key for inner and outer digest */
            for (int i = 0; i < KeyValue.Length; ++i) 
            {
                innerPad[i] = (byte)(KeyValue[i] ^ 0x36);
                outerPad[i] = (byte)(KeyValue[i] ^ 0x5C);
            }

            for (int i = KeyValue.Length; i < 64; ++i) 
            {
                innerPad[i] = 0x36;
                outerPad[i] = 0x5C;
            }
        }

        #endregion
    }
}

#endif
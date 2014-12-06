
namespace SecureSocketLayer.Net.Security.Providers.Common.Server
{
	internal sealed class ServerRecordDecryptor : RecordDecryptor
	{
		#region · Constructors ·

		public ServerRecordDecryptor(CipherSuite cipherSuite, ISecureKeyInfo keyInfo)
			:base(cipherSuite, keyInfo)
		{
		}

		#endregion

		#region · Protected Methods ·

		protected override void Initialize()
		{
			base.Initialize();

			// Set the key and IV for the algorithm
			this.DecryptionAlgorithm.Key = this.KeyInfo.ClientWriteKey;
			this.DecryptionAlgorithm.IV = this.KeyInfo.ClientWriteIV;

			// Create decryption cipher			
			this.DecryptionCipher = this.DecryptionAlgorithm.CreateDecryptor();
		}

		#endregion
	}
}

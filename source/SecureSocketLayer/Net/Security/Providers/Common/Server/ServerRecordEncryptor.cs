
namespace SecureSocketLayer.Net.Security.Providers.Common.Server
{
	internal sealed class ServerRecordEncryptor : RecordEncryptor
	{
		#region · Constructors ·

		public ServerRecordEncryptor(CipherSuite cipherSuite, ISecureKeyInfo keyInfo)
			:base(cipherSuite, keyInfo)
		{
		}

		#endregion

		#region · Private Methods ·

		protected override void Initialize()
		{
			base.Initialize();

			// Set the key and IV for the algorithm
			this.EncryptionAlgorithm.Key	= this.KeyInfo.ServerWriteKey;
			this.EncryptionAlgorithm.IV		= this.KeyInfo.ServerWriteIV;

			// Create encryption cipher
			this.EncryptionCipher = this.EncryptionAlgorithm.CreateEncryptor();
		}

		#endregion
	}
}

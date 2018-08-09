using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.UI;
using static Axantum.AxCrypt.Abstractions.TypeResolve;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Session;

namespace AxCrypt.Sdk
{
    public class SdkStreamEncryptor
    {
        private List<UserPublicKey> _publicKeys = new List<UserPublicKey>();

        private Passphrase _passphrase = Passphrase.Empty;

        private Guid _cryptoId;

        private AxCryptOptions _options;

        public SdkStreamEncryptor(SdkConfiguration configuration)
        {
            _cryptoId = configuration.CryptoId;
            _options = configuration.Copmress ? AxCryptOptions.EncryptWithCompression : AxCryptOptions.EncryptWithoutCompression;
            _options |= AxCryptOptions.SetFileTimes;
        }

        public SdkStreamEncryptor SetPassphrase(string passphrase)
        {
            _passphrase = new Passphrase(passphrase);

            return this;
        }

        public SdkStreamEncryptor AddPublicKey(string email, string pem)
        {
            UserPublicKey publicKey = new UserPublicKey(EmailAddress.Parse(email), New<IAsymmetricFactory>().CreatePublicKey(pem));
            _publicKeys.Add(publicKey);

            return this;
        }

        public async Task EncryptAsync(Stream clearIn, Stream encryptedOut, string fileName)
        {
            EncryptionParameters parameters = new EncryptionParameters(_cryptoId, _passphrase);
            await parameters.AddAsync(_publicKeys);

            EncryptedProperties properties = new EncryptedProperties(fileName);

            AxCryptFile.Encrypt(clearIn, encryptedOut, properties, parameters, _options, new ProgressContext());
        }
    }
}

using AxCrypt.Abstractions;
using AxCrypt.Api.Extension;
using AxCrypt.Api.Model;
using AxCrypt.Api.Model.Secret;
using AxCrypt.Core;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.Extensions;
using AxCrypt.Core.Secrets;
using AxCrypt.Cryptor.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Cryptor
{
    public static class TextCryptor
    {
        #region Private classes

        private class InternalSecret : Secret
        {
            public InternalSecret(Secret secret)
                : base(secret)
            {
            }

            private DateTime _lastUpdateUtc;

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "The getter is currently unused but should still be there.")]
            public DateTime LastUpdateUtc
            {
                get { return _lastUpdateUtc; }
                set { _lastUpdateUtc = value; }
            }
        }

        private class InternalEncryptionKey : EncryptionKey
        {
            public InternalEncryptionKey(EncryptionKey key)
                : base(key)
            {
            }

            public new string DecryptPassphrase()
            {
                return base.DecryptPassphrase();
            }
        }

        #endregion Private classes

        public static async Task<EncryptedSecretApiModel> Encrypt(LogOnIdentity identity, SecretsClientModel secretJson)
        {
            if (identity is null)
            {
                throw new ArgumentNullException(nameof(identity));
            }

            if (secretJson is null)
            {
                throw new ArgumentNullException(nameof(secretJson));
            }

            return await EncryptText(identity, secretJson);
        }

        public static async Task<EncryptedSecretApiModel> EncryptAsync(LogOnIdentity identity, SecretsClientModel secretJson, IEnumerable<UserPublicKey> sharedKeyHolders)
        {
            if (identity is null)
            {
                throw new ArgumentNullException(nameof(identity));
            }

            if (secretJson is null)
            {
                throw new ArgumentNullException(nameof(secretJson));
            }

            return await EncryptText(identity, secretJson, sharedKeyHolders);
        }

        private static async Task<EncryptedSecretApiModel> EncryptText(LogOnIdentity identity, SecretsClientModel secretJson, IEnumerable<UserPublicKey> sharedKeyHolders = null)
        {
            Guid cryptoId = Resolve.CryptoFactory.Default(New<ICryptoPolicy>()).CryptoId;
            EncryptionParameters encryptionParameters = new EncryptionParameters(cryptoId, identity);
            if (sharedKeyHolders != null)
            {
                await AddSharingParameters(encryptionParameters, sharedKeyHolders);
            }

            string userEmail = identity.UserEmail.Address;
            EncryptedSecretApiModel encryptedSecret = new EncryptedSecretApiModel()
            {
                UserEmail = userEmail,
                CreatedUtc = New<INow>().Utc
            };

            try
            {
                string serializedText = Serializer.Serialize(secretJson);
                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(serializedText);

                using (MemoryStream destinationStream = new MemoryStream())
                {
                    using (MemoryStream sourceStream = new MemoryStream(byteArray))
                    {
                        using (IAxCryptDocument document = New<AxCryptFactory>().CreateDocument(encryptionParameters))
                        {
                            document.FileName = $"{userEmail}.secrets";
                            document.CreationTimeUtc = New<INow>().Utc;
                            document.LastWriteTimeUtc = New<INow>().Utc;
                            document.EncryptTo(sourceStream, destinationStream, AxCryptOptions.EncryptWithCompression);
                        }
                    }
                    encryptedSecret.Cipher = destinationStream.ToArray();
                }
            }
            finally
            {
            }

            return encryptedSecret;
        }

        private static async Task AddSharingParameters(EncryptionParameters parameters, IEnumerable<UserPublicKey> sharedKeyHolders)
        {
            if (sharedKeyHolders == null || !sharedKeyHolders.Any())
            {
                return;
            }

            await parameters.AddAsync(sharedKeyHolders);
        }

        public static IEnumerable<SecretClientModel> GetClientSecrets(LogOnIdentity identity, EncryptedSecretApiModel encryptedSecret)
        {
            if (encryptedSecret.Cipher == null)
            {
                return Enumerable.Empty<SecretClientModel>();
            }

            SecretsClientModel secretsList = Decrypt(identity, encryptedSecret);
            if (secretsList == null)
            {
                return Enumerable.Empty<SecretClientModel>();
            }

            return secretsList.Secrets;
        }

        public static IEnumerable<Secret> GetSecrets(LogOnIdentity identity, EncryptedSecretApiModel encryptedSecret)
        {
            SecretsClientModel secretsList = Decrypt(identity, encryptedSecret);
            if (secretsList == null)
            {
                return Enumerable.Empty<Secret>();
            }

            EncryptionKey encryptionKey = new EncryptionKey(identity.Passphrase.Text);
            return secretsList.Secrets.Select(sec => { return GetSecret(sec, encryptionKey); });
        }

        public static SecretsClientModel DecryptAsync(LogOnIdentity identity, EncryptedSecretApiModel encryptedSecret, AxCrypt.Core.Service.UserKeyPair userKeyPair = null)
        {
            if (identity is null)
            {
                throw new ArgumentNullException(nameof(identity));
            }

            return Decrypt(identity, encryptedSecret);
        }

        private static SecretsClientModel Decrypt(LogOnIdentity identity, EncryptedSecretApiModel encryptedSecret)
        {
            string decryptedText = "";
            using (MemoryStream sourceStream = new MemoryStream(encryptedSecret.Cipher))
            {
                IAxCryptDocument document = Document(sourceStream, identity);
                if (!document.PassphraseIsValid)
                {
                    return new SecretsClientModel();
                }

                using (MemoryStream destinationStream = new MemoryStream())
                {
                    document.DecryptTo(destinationStream);

                    byte[] destinationBytes = destinationStream.ToArray();
                    decryptedText = System.Text.Encoding.UTF8.GetString(destinationBytes, 0, destinationBytes.Length);
                }
            }

            return Serializer.Deserialize<SecretsClientModel>(decryptedText);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "displayContext")]
        private static IAxCryptDocument Document(Stream source, LogOnIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException(nameof(identity));
            }

            IAxCryptDocument document = New<AxCryptFactory>().CreateDocument(identity.DecryptionParameters(), source);
            return document;
        }

        private static Secret GetSecret(SecretClientModel model, EncryptionKey key)
        {
            if (model == null)
            {
                return null;
            }

            Secret secret;
            switch (model.Type)
            {
                case SecretType.Legacy:
                case SecretType.Password:
                    secret = new Secret(model.Id, model.Password, key, model.CreatedUtc, model.UpdatedUtc, model.DeletedUtc);
                    break;

                case SecretType.Card:
                    secret = new Secret(model.Id, model.Card, key, model.CreatedUtc, model.UpdatedUtc, model.DeletedUtc);
                    break;

                case SecretType.Note:
                    secret = new Secret(model.Id, model.Note, key, model.CreatedUtc, model.UpdatedUtc, model.DeletedUtc);
                    break;

                default:
                    return null;
            }

            secret.DBId = model.DBId;

            return new InternalSecret(secret);
        }

        public static async Task<string> EncryptTextAsync(LogOnIdentity identity, string messageJson, IEnumerable<UserPublicKey> sharedKeyHolders)
        {
            if (identity is null)
            {
                throw new ArgumentNullException(nameof(identity));
            }

            if (messageJson is null)
            {
                throw new ArgumentNullException(nameof(messageJson));
            }

            return await InternalEncryptTextAsync(identity, messageJson, sharedKeyHolders);
        }

        private static async Task<string> InternalEncryptTextAsync(LogOnIdentity identity, string plainText, IEnumerable<UserPublicKey> sharedKeyHolders = null)
        {
            Guid cryptoId = Resolve.CryptoFactory.Default(New<ICryptoPolicy>()).CryptoId;
            EncryptionParameters encryptionParameters = new EncryptionParameters(cryptoId, identity);
            if (sharedKeyHolders != null)
            {
                await AddSharingParameters(encryptionParameters, sharedKeyHolders);
            }

            string userEmail = identity.UserEmail.Address;
            byte[] encryptedText;
            try
            {
                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(plainText);

                using (MemoryStream destinationStream = new MemoryStream())
                {
                    using (MemoryStream sourceStream = new MemoryStream(byteArray))
                    {
                        using (IAxCryptDocument document = New<AxCryptFactory>().CreateDocument(encryptionParameters))
                        {
                            document.FileName = $"{userEmail}.secrets";
                            document.CreationTimeUtc = New<INow>().Utc;
                            document.LastWriteTimeUtc = New<INow>().Utc;
                            document.EncryptTo(sourceStream, destinationStream, AxCryptOptions.EncryptWithCompression);
                        }
                    }
                    encryptedText = destinationStream.ToArray();
                }
            }
            finally
            {
            }

            return encryptedText.GetCipherString();
        }

        public static string DecryptTextAsync(LogOnIdentity identity, string encryptedText, AxCrypt.Core.Service.UserKeyPair userKeyPair = null)
        {
            if (identity is null)
            {
                throw new ArgumentNullException(nameof(identity));
            }

            return InternalDecryptTextAsync(identity, encryptedText, userKeyPair);
        }

        private static string InternalDecryptTextAsync(LogOnIdentity identity, string encryptedText, AxCrypt.Core.Service.UserKeyPair userKeyPair)
        {
            string decryptedText = "";
            using (MemoryStream sourceStream = new MemoryStream(encryptedText.GetCipherBytes()))
            {
                IAxCryptDocument document = Document(sourceStream, identity);
                if (!document.PassphraseIsValid)
                {
                    return null;
                }

                using (MemoryStream destinationStream = new MemoryStream())
                {
                    document.DecryptTo(destinationStream);

                    byte[] destinationBytes = destinationStream.ToArray();
                    decryptedText = System.Text.Encoding.UTF8.GetString(destinationBytes, 0, destinationBytes.Length);
                }
            }

            return decryptedText;
        }

        private static IStringSerializer Serializer
        {
            get
            {
                return New<IStringSerializer>();
            }
        }
    }
}
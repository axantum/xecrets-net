using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Service;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Extensions
{
    public static class UserTypeExtensions
    {
        public static string ToFileString(this PublicKeyThumbprint thumbprint)
        {
            if (thumbprint == null)
            {
                throw new ArgumentNullException("thumbprint");
            }

            string base64 = Convert.ToBase64String(thumbprint.ToByteArray());
            string fileString = base64.Substring(0, base64.Length - 2).Replace('/', '-');

            return fileString;
        }

        public static PublicKeyThumbprint ToPublicKeyThumbprint(this string thumbprint)
        {
            if (thumbprint == null)
            {
                throw new ArgumentNullException("thumbprint");
            }
            if (thumbprint.Length != 22)
            {
                throw new ArgumentException("Length must be 128 bits base 64 without padding.", "thumbprint");
            }
            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(thumbprint.Replace('-', '/') + "==");
            }
            catch (FormatException)
            {
                throw new ArgumentException("Incorrect base64 encoding.", "thumbprint");
            }

            return new PublicKeyThumbprint(bytes);
        }

        /// <summary>
        /// Convert the internal representation of a key pair to the external account key representation.
        /// </summary>
        /// <param name="keys">The key pair.</param>
        /// <param name="passphrase">The passphrase to encrypt it with.</param>
        /// <returns>A representation suitable for serialization and external storage.</returns>
        public static Api.Model.AccountKey ToAccountKey(this UserKeyPair keys, Passphrase passphrase)
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            string encryptedPrivateKey = EncryptPrivateKey(keys, passphrase);

            Api.Model.KeyPair keyPair = new Api.Model.KeyPair(keys.KeyPair.PublicKey.ToString(), encryptedPrivateKey);
            Api.Model.AccountKey accountKey = new Api.Model.AccountKey(keys.UserEmail.Address, keys.KeyPair.PublicKey.Thumbprint.ToString(), keyPair, keys.Timestamp);

            return accountKey;
        }

        private static string EncryptPrivateKey(UserKeyPair keys, Passphrase passphrase)
        {
            if (keys.KeyPair.PrivateKey == null)
            {
                return String.Empty;
            }

            byte[] privateKeyPemBytes = Encoding.UTF8.GetBytes(keys.KeyPair.PrivateKey.ToString());

            if (passphrase == Passphrase.Empty)
            {
                byte[] encryptedPrivateKeyBytes = New<IDataProtection>().Protect(privateKeyPemBytes);
                return Convert.ToBase64String(encryptedPrivateKeyBytes);
            }

            StringBuilder encryptedPrivateKey = new StringBuilder();
            using (StringWriter writer = new StringWriter(encryptedPrivateKey))
            {
                using (Stream stream = new MemoryStream(privateKeyPemBytes))
                {
                    EncryptionParameters encryptionParameters = new EncryptionParameters(Resolve.CryptoFactory.Preferred.Id, passphrase);
                    EncryptedProperties properties = new EncryptedProperties("private-key.pem");
                    using (MemoryStream encryptedStream = new MemoryStream())
                    {
                        AxCryptFile.Encrypt(stream, encryptedStream, properties, encryptionParameters, AxCryptOptions.EncryptWithCompression, new ProgressContext());
                        writer.Write(Convert.ToBase64String(encryptedStream.ToArray()));
                    }
                }
            }
            return encryptedPrivateKey.ToString();
        }

        /// <summary>
        /// Convert an external representation of a key-pair to an internal representation that is suitable for actual use.
        /// </summary>
        /// <param name="accountKey">The account key.</param>
        /// <param name="passphrase">The passphrase to decrypt the private key, if any, with.</param>
        /// <returns></returns>
        public static UserKeyPair ToUserAsymmetricKeys(this Api.Model.AccountKey accountKey, Passphrase passphrase)
        {
            if (accountKey == null)
            {
                throw new ArgumentNullException(nameof(accountKey));
            }

            string privateKeyPem = DecryptPrivateKeyPem(accountKey.KeyPair.PrivateEncryptedPem, passphrase);
            if (privateKeyPem == null)
            {
                return null;
            }

            IAsymmetricKeyPair keyPair = Resolve.AsymmetricFactory.CreateKeyPair(accountKey.KeyPair.PublicPem, privateKeyPem);
            UserKeyPair userAsymmetricKeys = new UserKeyPair(EmailAddress.Parse(accountKey.User), accountKey.Timestamp, keyPair);

            return userAsymmetricKeys;
        }

        private static string DecryptPrivateKeyPem(string privateEncryptedPem, Passphrase passphrase)
        {
            if (privateEncryptedPem.Length == 0)
            {
                return String.Empty;
            }

            byte[] privateKeyEncryptedPem = Convert.FromBase64String(privateEncryptedPem);

            if (passphrase == Passphrase.Empty)
            {
                byte[] decryptedPrivateKeyBytes = New<IDataProtection>().Unprotect(privateKeyEncryptedPem);
                return Encoding.UTF8.GetString(decryptedPrivateKeyBytes, 0, decryptedPrivateKeyBytes.Length);
            }

            using (MemoryStream encryptedPrivateKeyStream = new MemoryStream(privateKeyEncryptedPem))
            {
                using (MemoryStream decryptedPrivateKeyStream = new MemoryStream())
                {
                    DecryptionParameter decryptionParameter = new DecryptionParameter(passphrase, Resolve.CryptoFactory.Preferred.Id);
                    if (!New<AxCryptFile>().Decrypt(encryptedPrivateKeyStream, decryptedPrivateKeyStream, new DecryptionParameter[] { decryptionParameter }).IsValid)
                    {
                        return null;
                    }

                    return Encoding.UTF8.GetString(decryptedPrivateKeyStream.ToArray(), 0, (int)decryptedPrivateKeyStream.Length);
                }
            }
        }
    }
}
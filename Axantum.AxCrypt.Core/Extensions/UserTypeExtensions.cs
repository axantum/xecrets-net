using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

        public static Api.Model.AccountKey ToAccountKey(this UserAsymmetricKeys keys, Passphrase passphrase)
        {
            StringBuilder encryptedPrivateKey = new StringBuilder();
            using (StringWriter writer = new StringWriter(encryptedPrivateKey))
            {
                string privateKeyPem = keys.KeyPair.PrivateKey.ToString();
                using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(privateKeyPem)))
                {
                    EncryptionParameters encryptionParameters = new EncryptionParameters(V2Aes256CryptoFactory.CryptoId, passphrase);
                    EncryptedProperties properties = new EncryptedProperties("private-key.pem");
                    using (MemoryStream exportStream = new MemoryStream())
                    {
                        AxCryptFile.Encrypt(stream, exportStream, properties, encryptionParameters, AxCryptOptions.EncryptWithCompression, new ProgressContext());
                        writer.Write(Convert.ToBase64String(exportStream.ToArray()));
                    }
                }
            }

            Api.Model.KeyPair keyPair = new Api.Model.KeyPair(keys.KeyPair.PublicKey.ToString(), encryptedPrivateKey.ToString());
            Api.Model.AccountKey accountKey = new Api.Model.AccountKey(keys.UserEmail.Address, keys.KeyPair.PublicKey.Thumbprint.ToString(), keyPair, keys.Timestamp);

            return accountKey;
        }

        public static UserAsymmetricKeys ToUserAsymmetricKeys(this Api.Model.AccountKey accountKey, Passphrase passphrase)
        {
            string privateKeyPem;
            byte[] privateKeyEncryptedBytes = Convert.FromBase64String(accountKey.KeyPair.PrivateBytes);
            using (MemoryStream encryptedStream = new MemoryStream(privateKeyEncryptedBytes))
            {
                using (MemoryStream decryptedStream = new MemoryStream())
                {
                    if (!TypeMap.Resolve.New<AxCryptFile>().Decrypt(encryptedStream, decryptedStream, new LogOnIdentity(passphrase)).IsValid)
                    {
                        return null;
                    }

                    privateKeyPem = Encoding.UTF8.GetString(decryptedStream.ToArray(), 0, (int)decryptedStream.Length);
                }
            }

            IAsymmetricKeyPair keyPair = Resolve.AsymmetricFactory.CreateKeyPair(privateKeyPem, accountKey.KeyPair.PublicBytes);
            UserAsymmetricKeys userAsymmetricKeys = new UserAsymmetricKeys(EmailAddress.Parse(accountKey.User), accountKey.Timestamp, keyPair);

            return userAsymmetricKeys;
        }
    }
}
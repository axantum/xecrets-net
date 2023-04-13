#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
 *
 * This file is part of AxCrypt.
 *
 * AxCrypt is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * AxCrypt is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AxCrypt.  If not, see <http://www.gnu.org/licenses/>.
 *
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Abstractions;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.Extensions;
using AxCrypt.Core.IO;
using AxCrypt.Core.Session;
using AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Service
{
    /// <summary>
    /// A respository for a single user email address. A user has a single active key pair, with both a public
    /// key for encryption and the matching private key for decryption.
    /// </summary>
    /// <remarks>Instances of this type are immutable.</remarks>
    public class UserKeyPair : IEquatable<UserKeyPair>
    {
        private UserKeyPair(EmailAddress emailAddress)
        {
            UserEmail = emailAddress;
        }

        public static readonly UserKeyPair Empty = new UserKeyPair(EmailAddress.Empty);

        public UserKeyPair(EmailAddress userEmail, int bits)
            : this(userEmail)
        {
            Timestamp = New<INow>().Utc;
            UserEmail = userEmail;
            KeyPair = New<IAsymmetricFactory>().CreateKeyPair(bits);
        }

        public UserKeyPair(EmailAddress userEmail, DateTime timestamp, IAsymmetricKeyPair keyPair)
        {
            UserEmail = userEmail;
            Timestamp = timestamp;
            KeyPair = keyPair;
        }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; private set; }

        [JsonPropertyName("useremail")]
        public EmailAddress UserEmail { get; private set; }

        [JsonPropertyName("keypair"), AllowNull]
        public IAsymmetricKeyPair KeyPair { get; private set; }

        /// <summary>
        /// Loads the specified key pairs for the given user from the provided set of candiates in the form of IDataStore instances.
        /// </summary>
        /// <param name="stores">The stores.</param>
        /// <param name="userEmail">The user email.</param>
        /// <param name="passphrase">The passphrase.</param>
        /// <returns>Valid key pairs for the user.</returns>
        public static IEnumerable<UserKeyPair> Load(IEnumerable<IDataStore> stores, EmailAddress userEmail, Passphrase passphrase)
        {
            if (stores == null)
            {
                throw new ArgumentNullException(nameof(stores));
            }

            List<UserKeyPair> userKeyPairs = new List<UserKeyPair>();
            foreach (IDataStore store in stores)
            {
                if (!TryLoad(store.ToArray(), passphrase, out UserKeyPair? userKeyPair))
                {
                    continue;
                }
                if (userEmail != userKeyPair!.UserEmail)
                {
                    continue;
                }
                userKeyPairs.Add(userKeyPair);
            }
            return userKeyPairs;
        }

        private const string FileFormat = "Keys-{0}.txt";

        public byte[] ToArray(Passphrase passphrase)
        {
            return GetSaveDataForKeys(this, FileFormat.InvariantFormat(KeyPair.PublicKey.Tag), passphrase);
        }

        /// <summary>
        /// Tries to load a key pair from the serialized byte array.
        /// </summary>
        /// <param name="value">The bytes.</param>
        /// <param name="passphrase">The passphrase.</param>
        /// <param name="keyPair">The key pair.</param>
        /// <returns>True if the pair was successfully loaded, and set in the keyPair parameter.</returns>
        public static bool TryLoad(byte[] value, Passphrase passphrase, out UserKeyPair? keyPair)
        {
            using var encryptedStream = new MemoryStream(value);
            using var decryptedStream = new MemoryStream();

            EncryptedProperties? properties = New<AxCryptFile>().Decrypt(encryptedStream, decryptedStream, new DecryptionParameter[] { new DecryptionParameter(passphrase, Resolve.CryptoFactory.Preferred.CryptoId) });
            if (!properties!.IsValid)
            {
                keyPair = null;
                return false;
            }

            string json = Encoding.UTF8.GetString(decryptedStream.ToArray(), 0, (int)decryptedStream.Length);
            keyPair = Resolve.Serializer.Deserialize<UserKeyPair>(json);
            return true;
        }

        private static byte[] GetSaveDataForKeys(UserKeyPair keys, string originalFileName, Passphrase passphrase)
        {
            string json = Resolve.Serializer.Serialize(keys);
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            EncryptionParameters encryptionParameters = new EncryptionParameters(Resolve.CryptoFactory.Preferred.CryptoId, passphrase);
            EncryptedProperties properties = new EncryptedProperties(originalFileName);
            using var exportStream = new MemoryStream();
            AxCryptFile.Encrypt(stream, exportStream, properties, encryptionParameters, AxCryptOptions.EncryptWithCompression, new ProgressContext());
            return exportStream.ToArray();
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as UserKeyPair);
        }

        public override int GetHashCode()
        {
            return Timestamp.GetHashCode() ^ UserEmail.GetHashCode() ^ (KeyPair == null ? 0 : KeyPair.GetHashCode());
        }

        public static bool operator ==(UserKeyPair? left, UserKeyPair? right)
        {
            if (Object.ReferenceEquals(left, null))
            {
                return Object.ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(UserKeyPair? left, UserKeyPair? right)
        {
            return !(left == right);
        }

        public bool Equals(UserKeyPair? other)
        {
            if (other is null || GetType() != other.GetType())
            {
                return false;
            }
            if (ReferenceEquals(other, this))
            {
                return true;
            }

            return Timestamp == other.Timestamp && UserEmail == other.UserEmail && KeyPair.Equals(other.KeyPair);
        }
    }
}

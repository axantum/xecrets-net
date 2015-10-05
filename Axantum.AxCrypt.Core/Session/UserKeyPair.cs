using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Session
{
    /// <summary>
    /// A respository for a single user e-mail. A user has a single active key pair, with both a public
    /// key for encryption and the matching private key for decryption.
    /// </summary>
    /// <remarks>Instances of this type are immutable.</remarks>
    [JsonObject(MemberSerialization.OptIn)]
    public class UserKeyPair : IEquatable<UserKeyPair>
    {
        [JsonConstructor]
        private UserKeyPair(EmailAddress emailAddress)
        {
            UserEmail = emailAddress;
        }

        public static readonly UserKeyPair Empty = new UserKeyPair(EmailAddress.Empty);

        public UserKeyPair(EmailAddress userEmail, int bits)
            : this(userEmail)
        {
            Timestamp = Resolve.Environment.UtcNow;
            UserEmail = userEmail;
            KeyPair = TypeMap.Resolve.Singleton<IAsymmetricFactory>().CreateKeyPair(bits);
        }

        public UserKeyPair(EmailAddress userEmail, DateTime timestamp, IAsymmetricKeyPair keyPair)
        {
            UserEmail = userEmail;
            Timestamp = timestamp;
            KeyPair = keyPair;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used by Json.NET serializer.")]
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; private set; }

        [JsonProperty("useremail")]
        [JsonConverter(typeof(EmailAddressJsonConverter))]
        public EmailAddress UserEmail { get; private set; }

        [JsonProperty("keypair")]
        public IAsymmetricKeyPair KeyPair { get; private set; }

        private const string _fileFormat = "Keys-{0}.txt";

        public byte[] ToArray(Passphrase passphrase)
        {
            return GetSaveDataForKeys(this, _fileFormat.InvariantFormat(KeyPair.PublicKey.Tag), passphrase);
        }

        private static byte[] GetSaveDataForKeys(UserKeyPair keys, string originalFileName, Passphrase passphrase)
        {
            string json = Resolve.Serializer.Serialize(keys);
            using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                EncryptionParameters encryptionParameters = new EncryptionParameters(Resolve.CryptoFactory.Preferred.Id, passphrase);
                EncryptedProperties properties = new EncryptedProperties(originalFileName);
                using (MemoryStream exportStream = new MemoryStream())
                {
                    AxCryptFile.Encrypt(stream, exportStream, properties, encryptionParameters, AxCryptOptions.EncryptWithCompression, new ProgressContext());
                    return exportStream.ToArray();
                }
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as UserKeyPair);
        }

        public override int GetHashCode()
        {
            return Timestamp.GetHashCode() ^ UserEmail.GetHashCode() ^ (KeyPair == null ? 0 : KeyPair.GetHashCode());
        }

        public static bool operator ==(UserKeyPair left, UserKeyPair right)
        {
            if (Object.ReferenceEquals(left, null))
            {
                return Object.ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(UserKeyPair left, UserKeyPair right)
        {
            return !(left == right);
        }

        public bool Equals(UserKeyPair other)
        {
            if (Object.ReferenceEquals(other, null) || GetType() != other.GetType())
            {
                return false;
            }
            if (Object.ReferenceEquals(other, this))
            {
                return true;
            }

            return Timestamp == other.Timestamp && UserEmail == other.UserEmail && KeyPair.Equals(KeyPair);
        }
    }
}
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    public class UserAsymmetricKeys : IEquatable<UserAsymmetricKeys>
    {
        [JsonConstructor]
        private UserAsymmetricKeys()
        {
        }

        public static readonly UserAsymmetricKeys Empty = new UserAsymmetricKeys { UserEmail = EmailAddress.Empty, };

        public UserAsymmetricKeys(EmailAddress userEmail, int bits)
            : this()
        {
            Timestamp = Resolve.Environment.UtcNow;
            UserEmail = userEmail;
            KeyPair = TypeMap.Resolve.Singleton<IAsymmetricFactory>().CreateKeyPair(bits);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used by Json.NET serializer.")]
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; private set; }

        [JsonProperty("useremail")]
        [JsonConverter(typeof(EmailAddressJsonConverter))]
        public EmailAddress UserEmail { get; private set; }

        [JsonProperty("keypair")]
        public IAsymmetricKeyPair KeyPair { get; private set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as UserAsymmetricKeys);
        }

        public override int GetHashCode()
        {
            return Timestamp.GetHashCode() ^ UserEmail.GetHashCode() ^ (KeyPair == null ? 0 : KeyPair.GetHashCode());
        }

        public static bool operator ==(UserAsymmetricKeys left, UserAsymmetricKeys right)
        {
            if (Object.ReferenceEquals(left, null))
            {
                return Object.ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(UserAsymmetricKeys left, UserAsymmetricKeys right)
        {
            return !(left == right);
        }

        public bool Equals(UserAsymmetricKeys other)
        {
            if (Object.ReferenceEquals(other, null) || GetType() != other.GetType())
            {
                return false;
            }

            return Timestamp == other.Timestamp && UserEmail == other.UserEmail && KeyPair == other.KeyPair;
        }
    }
}
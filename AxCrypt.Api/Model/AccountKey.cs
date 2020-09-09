using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxCrypt.Api.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AccountKey : IEquatable<AccountKey>
    {
        public static readonly AccountKey Empty = new AccountKey(String.Empty, String.Empty, KeyPair.Empty, DateTime.MinValue, PrivateKeyStatus.PassphraseUnknown);

        public AccountKey(string user, string thumbprint, KeyPair keyPair, DateTime timestamp, PrivateKeyStatus status)
        {
            User = user ?? throw new ArgumentNullException("user");
            Thumbprint = thumbprint ?? throw new ArgumentNullException("thumbprint");
            KeyPair = keyPair ?? throw new ArgumentNullException("keyPair");

            Timestamp = timestamp;
            Status = status;
        }

        /// <summary>
        /// Sets the private key status.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <returns>A new instance with the status set to the provided value.</returns>
        public AccountKey SetStatus(PrivateKeyStatus status)
        {
            AccountKey newKey = new AccountKey(User, Thumbprint, KeyPair, Timestamp, status);
            return newKey;
        }

        /// <summary>
        /// Gets the timestamp, when the key pair was generated. The thumbprint
        /// remains as the unique identifier to recognize identical key pairs, but the timestamp
        /// determines the most recently generated, and the most recent should be preferred.
        /// </summary>
        /// <value>
        /// The timestamp.
        /// </value>
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; }

        /// <summary>
        /// Gets the thumbprint.
        /// </summary>
        /// <value>
        /// The thumbprint of the public key in the key pair for identification purposes.
        /// </value>
        [JsonProperty("thumbprint")]
        public string Thumbprint { get; }

        /// <summary>
        /// Gets the key pair. If the KeyPair is Empty, it has been permanently deleted including the private key.
        /// </summary>
        /// <value>
        /// The key pair.
        /// </value>
        [JsonProperty("keypair")]
        public KeyPair KeyPair { get; }

        /// <summary>
        /// Gets the status of this key pair
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        [JsonProperty("keypair_status")]
        public PrivateKeyStatus Status { get; } 

        /// <summary>
        /// Gets the user name, typically the email address.
        /// </summary>
        /// <value>
        /// The user name (email address).
        /// </value>
        [JsonProperty("user")]
        public string User { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty
        {
            get
            {
                return Thumbprint.Length == 0 && KeyPair.IsEmpty;
            }
        }

        public bool Equals(AccountKey other)
        {
            if ((object)other == null)
            {
                return false;
            }

            return Timestamp == other.Timestamp && User == other.User && Status == other.Status && Thumbprint == other.Thumbprint && KeyPair == other.KeyPair;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || typeof(AccountKey) != obj.GetType())
            {
                return false;
            }
            AccountKey other = (AccountKey)obj;

            return Equals(other);
        }

        public override int GetHashCode()
        {
            return Timestamp.GetHashCode() ^ User.GetHashCode() ^ Status.GetHashCode() ^ Thumbprint.GetHashCode() ^ KeyPair.GetHashCode();
        }

        public static bool operator ==(AccountKey left, AccountKey right)
        {
            if (Object.ReferenceEquals(left, right))
            {
                return true;
            }
            if ((object)left == null)
            {
                return false;
            }
            return left.Equals(right);
        }

        public static bool operator !=(AccountKey left, AccountKey right)
        {
            return !(left == right);
        }
    }
}
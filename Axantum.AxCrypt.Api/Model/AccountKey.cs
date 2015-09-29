using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Api.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AccountKey : IEquatable<AccountKey>
    {
        public static readonly AccountKey Empty = new AccountKey();

        public AccountKey()
            : this(String.Empty, KeyPair.Empty, DateTime.MinValue)
        {
        }

        public AccountKey(string thumbprint, KeyPair keyPair, DateTime timestamp)
        {
            if (thumbprint == null)
            {
                throw new ArgumentNullException("thumbprint");
            }
            if (keyPair == null)
            {
                throw new ArgumentNullException("keyPair");
            }

            Thumbprint = thumbprint;
            KeyPair = keyPair;
            Timestamp = timestamp;
        }

        /// <summary>
        /// Gets the timestamp, when the key pair was encrypted (not generated). The thumbprint
        /// remains as the unique identifier to recognize identical key pairs, but the timestamp
        /// determines the most recent encryption, i.e. the most recent should be preferred.
        /// </summary>
        /// <value>
        /// The timestamp.
        /// </value>
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; private set; }

        /// <summary>
        /// Gets the thumbprint.
        /// </summary>
        /// <value>
        /// The thumbprint of the public key in the key pair for identification purposes.
        /// </value>
        [JsonProperty("thumbprint")]
        public string Thumbprint { get; private set; }

        [JsonProperty("keypair")]
        public KeyPair KeyPair { get; private set; }

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

            return Timestamp == other.Timestamp && Thumbprint == other.Thumbprint && KeyPair == other.KeyPair;
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
            return Timestamp.GetHashCode() ^ Thumbprint.GetHashCode() ^ KeyPair.GetHashCode();
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
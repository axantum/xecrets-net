using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Api.Model
{
    /// <summary>
    /// A summary of information we know about a user, typically fetched at the start of a session.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class UserAccount : IEquatable<UserAccount>
    {
        public UserAccount(string userName, SubscriptionLevel level, DateTime expiration, AccountStatus status, Offers offers, IEnumerable<AccountKey> keys)
        {
            UserName = userName;
            SubscriptionLevel = level;
            LevelExpiration = expiration;
            AccountKeys = keys.ToList();
            AccountStatus = status;
            Offers = offers;
            Tag = string.Empty;
            Signature = string.Empty;
            AccountSource = AccountSource.Unknown;
            CanTryAppStorePremiumTrial = false;
            ActiveSubscriptionFromAppStore = false;
        }

        public UserAccount(string userName, SubscriptionLevel level, DateTime expiration, AccountStatus status, Offers offers)
            : this(userName, level, expiration, status, offers, new AccountKey[0])
        {
        }

        public UserAccount(string userName, SubscriptionLevel level, AccountStatus status, Offers offers)
            : this(userName, level, DateTime.MinValue, status, offers)
        {
        }

        public UserAccount(string userName, SubscriptionLevel level, AccountStatus status)
            : this(userName, level, DateTime.MinValue, status, Offers.None)
        {
        }

        public UserAccount(string userName)
            : this(userName, SubscriptionLevel.Unknown, AccountStatus.Unknown, Offers.None)
        {
        }

        public UserAccount()
            : this(String.Empty)
        {
        }

        /// <summary>
        /// Gets the account keys.
        /// </summary>
        /// <value>
        /// The account keys.
        /// </value>
        [JsonProperty("keys")]
        public IList<AccountKey> AccountKeys { get; private set; }

        /// <summary>
        /// Gets the email address of the user.
        /// </summary>
        /// <value>
        /// The email address.
        /// </value>
        [JsonProperty("user")]
        public string UserName { get; private set; }

        /// <summary>
        /// Gets the currently valid subscription level.
        /// </summary>
        /// <value>
        /// The subscription level. Valid values are "" (unknown), "Free" and "Premium".
        /// </value>
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("level")]
        public SubscriptionLevel SubscriptionLevel { get; private set; }

        /// <summary>
        /// Gets the level expiration date and time UTC.
        /// </summary>
        /// <value>
        /// The level expiration date and time UTC.
        /// </value>
        [JsonProperty("expiration")]
        public DateTime LevelExpiration { get; private set; }

        /// <summary>
        /// Gets the offers the user has accepted already.
        /// </summary>
        [JsonProperty("offers")]
        public Offers Offers { get; private set; }

        /// <summary>
        /// Gets the account status.
        /// </summary>
        /// <value>
        /// The status (Unknown, Unverified, Verified).
        /// </value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Serialization")]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("status")]
        public AccountStatus AccountStatus { get; private set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }

        [JsonProperty("canTryAppStorePremiumTrial")]
        public bool CanTryAppStorePremiumTrial { get; set; }

        [JsonProperty("activeSubscriptionFromAppStore")]
        public bool ActiveSubscriptionFromAppStore { get; set; }

        public AccountSource AccountSource { get; set; }

        public bool Equals(UserAccount other)
        {
            if ((object)other == null)
            {
                return false;
            }

            if (UserName != other.UserName)
            {
                return false;
            }
            if (SubscriptionLevel != other.SubscriptionLevel)
            {
                return false;
            }
            if (LevelExpiration != other.LevelExpiration)
            {
                return false;
            }
            if (AccountStatus != other.AccountStatus)
            {
                return false;
            }
            if (Signature != other.Signature)
            {
                return false;
            }
            return AccountKeys.SequenceEqual(other.AccountKeys);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || typeof(UserAccount) != obj.GetType())
            {
                return false;
            }
            UserAccount other = (UserAccount)obj;

            return Equals(other);
        }

        public override int GetHashCode()
        {
            return AccountKeys.GetHashCode() ^ UserName.GetHashCode() ^ SubscriptionLevel.GetHashCode() ^ LevelExpiration.GetHashCode() ^ AccountStatus.GetHashCode() ^ AccountKeys.Aggregate(0, (sum, ak) => sum ^ ak.GetHashCode());
        }

        public static bool operator ==(UserAccount left, UserAccount right)
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

        public static bool operator !=(UserAccount left, UserAccount right)
        {
            return !(left == right);
        }
    }
}
using AxCrypt.Api.Model.Masterkey;
using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using JsonIgnoreAttribute = System.Text.Json.Serialization.JsonIgnoreAttribute;

namespace AxCrypt.Api.Model
{
    /// <summary>
    /// A summary of information we know about a user, typically fetched at the start of a session.
    /// </summary>
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
        [JsonPropertyName("keys")]
        public IList<AccountKey> AccountKeys { get; set; }

        /// <summary>
        /// Gets the email address of the user.
        /// </summary>
        /// <value>
        /// The email address.
        /// </value>
        [JsonPropertyName("user")]
        public string UserName { get; set; }

        /// <summary>
        /// Gets the currently valid subscription level.
        /// </summary>
        /// <value>
        /// The subscription level. Valid values are "" (unknown), "Free" and "Premium".
        /// </value>
        [JsonPropertyName("level")]
        public SubscriptionLevel SubscriptionLevel { get; set; }

        /// <summary>
        /// Gets the level expiration date and time UTC.
        /// </summary>
        /// <value>
        /// The level expiration date and time UTC.
        /// </value>
        [JsonPropertyName("expiration")]
        public DateTime LevelExpiration { get; set; }

        /// <summary>
        /// Gets the offers the user has accepted already.
        /// </summary>
        [JsonPropertyName("offers")]
        public Offers Offers { get; set; }

        /// <summary>
        /// Gets the account status.
        /// </summary>
        /// <value>
        /// The status (Unknown, Unverified, Verified).
        /// </value>
        [JsonPropertyName("status")]
        public AccountStatus AccountStatus { get; set; }

        [JsonPropertyName("tag")]
        public string Tag { get; set; }

        [JsonPropertyName("signature")]
        public string Signature { get; set; }

        [JsonPropertyName("canTryAppStorePremiumTrial")]
        public bool CanTryAppStorePremiumTrial { get; set; }

        [JsonPropertyName("activeSubscriptionFromAppStore")]
        public bool ActiveSubscriptionFromAppStore { get; set; }

        [JsonPropertyName("isMasterKeyEnabled")]
        public bool IsMasterKeyEnabled { get; set; }

        [JsonPropertyName("masterKey")]
        public MasterKeyPairInfo? MasterKeyPair { get; set; }

        [JsonIgnore]
        public AccountSource AccountSource { get; set; }

        [JsonPropertyName("businessAdmin")]
        public bool BusinessAdmin { get; set; }

        [JsonPropertyName("hadAnyPaidSubscription")]
        public bool HadAnyPaidSubscription { get; set; }

        public bool Equals(UserAccount? other)
        {
            if ((object?)other == null)
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
            if (MasterKeyPair! != other.MasterKeyPair!)
            {
                return false;
            }
            if (BusinessAdmin != other.BusinessAdmin)
            {
                return false;
            }
            return AccountKeys.SequenceEqual(other.AccountKeys);
        }

        public override bool Equals(object? obj)
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
            return MasterKeyPair!.GetHashCode() ^ AccountKeys.GetHashCode() ^ UserName.GetHashCode() ^ SubscriptionLevel.GetHashCode() ^ LevelExpiration.GetHashCode() ^ AccountStatus.GetHashCode() ^ AccountKeys.Aggregate(0, (sum, ak) => sum ^ ak.GetHashCode());
        }

        public static bool operator ==(UserAccount? left, UserAccount? right)
        {
            if (Object.ReferenceEquals(left, right))
            {
                return true;
            }
            if ((object?)left == null)
            {
                return false;
            }
            return left.Equals(right);
        }

        public static bool operator !=(UserAccount? left, UserAccount? right)
        {
            return !(left == right);
        }
    }
}

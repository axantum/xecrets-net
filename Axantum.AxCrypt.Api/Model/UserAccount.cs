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
    public class UserAccount
    {
        public UserAccount(string userName, SubscriptionLevel level, IEnumerable<AccountKey> accountKeys)
        {
            UserName = userName;
            SubscriptionLevel = level;
            AccountKeys = accountKeys.ToList();
        }

        public UserAccount()
            : this(String.Empty, SubscriptionLevel.Unknown, new AccountKey[0])
        {
        }

        /// <summary>
        /// Gets the public key.
        /// </summary>
        /// <value>
        /// The public key.
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
        /// Gets the account status.
        /// </summary>
        /// <value>
        /// The status (Unknown, Unverified, Verified).
        /// </value>
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("status")]
        public AccountStatus AccountStatus { get; private set; }
    }
}
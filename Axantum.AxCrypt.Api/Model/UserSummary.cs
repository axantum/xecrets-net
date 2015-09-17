using Newtonsoft.Json;
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
    public class UserSummary : CommonResponse
    {
        public UserSummary(string userName, string level, IEnumerable<string> thumbprints)
        {
            UserName = userName;
            PublicKeyThumbprints = thumbprints;
            SubscriptionLevel = level;
        }

        public UserSummary()
            : this(String.Empty, String.Empty, new string[0])
        {
        }

        /// <summary>
        /// Gets the email address of the user.
        /// </summary>
        /// <value>
        /// The email address.
        /// </value>
        [JsonProperty("user")]
        public string UserName { get; private set; }

        /// <summary>
        /// Gets the known public key thumbprints for this user in Base64 encoding.
        /// </summary>
        /// <value>
        /// The public key thumbprints without any particular order.
        /// </value>
        [JsonProperty("thumbprints")]
        public IEnumerable<string> PublicKeyThumbprints { get; private set; }

        /// <summary>
        /// Gets the currently valid subscription level.
        /// </summary>
        /// <value>
        /// The subscription level. Valid values are "" (unknown), "Free" and "Premium".
        /// </value>
        [JsonProperty("level")]
        public string SubscriptionLevel { get; private set; }
    }
}
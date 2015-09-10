using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Api.Model
{
    /// <summary>
    /// A summary of information we know about a user, typically fetched at the start of a session.
    /// </summary>
    public class UserSummary
    {
        public UserSummary()
        {
            EmailAddress = EmailAddress.Empty;
            PublicKeyThumbprints = new PublicKeyThumbprint[0];
            SubscriptionLevel = SubscriptionLevel.None;
        }

        /// <summary>
        /// Gets the email address of the user.
        /// </summary>
        /// <value>
        /// The email address.
        /// </value>
        public EmailAddress EmailAddress { get; private set; }

        /// <summary>
        /// Gets the known public key thumbprints for this user.
        /// </summary>
        /// <value>
        /// The public key thumbprints without any particular order.
        /// </value>
        public IEnumerable<PublicKeyThumbprint> PublicKeyThumbprints { get; private set; }

        /// <summary>
        /// Gets the currently valid subscription level.
        /// </summary>
        /// <value>
        /// The subscription level.
        /// </value>
        public SubscriptionLevel SubscriptionLevel { get; private set; }
    }
}
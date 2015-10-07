using Axantum.AxCrypt.Abstractions.Rest;
using Axantum.AxCrypt.Api.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Service
{
    /// <summary>
    /// The account service. Methods and properties to work with an account.
    /// </summary>
    public interface IAccountService
    {
        /// <summary>
        /// Gets the identity this instance works with.
        /// </summary>
        /// <value>
        /// The identity.
        /// </value>
        RestIdentity Identity { get; }

        /// <summary>
        /// Gets the subscription level.
        /// </summary>
        /// <value>
        /// The level.
        /// </value>
        SubscriptionLevel Level { get; }

        /// <summary>
        /// Gets the status of the account.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        AccountStatus Status { get; }

        /// <summary>
        /// Lists all UserKeyPairs available for the user.
        /// </summary>
        /// <returns></returns>
        IList<UserKeyPair> List();

        /// <summary>
        /// Saves the specified key pairs.
        /// </summary>
        /// <param name="keyPairs">The key pairs.</param>
        void Save(IEnumerable<UserKeyPair> keyPairs);

        /// <summary>
        /// Changes the passphrase for the account.
        /// </summary>
        /// <param name="passphrase">The passphrase.</param>
        /// <returns>true if the passphrase was successfully changed.</returns>
        bool ChangePassphrase(string passphrase);

        /// <summary>
        /// Gets a value indicating whether the service has any accounts at all.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has accounts; otherwise, <c>false</c>.
        /// </value>
        bool HasAccounts { get; }
    }
}
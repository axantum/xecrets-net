using Axantum.AxCrypt.Abstractions.Rest;
using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Core.Session;
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
        IEnumerable<UserKeyPair> List();

        /// <summary>
        /// Saves the specified key pairs.
        /// </summary>
        /// <param name="keyPairs">The key pairs.</param>
        void Save(IEnumerable<UserKeyPair> keyPairs);

        /// <summary>
        /// Changes the password for the account.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns>true if the password was successfully changed.</returns>
        bool ChangePassword(string password);
    }
}
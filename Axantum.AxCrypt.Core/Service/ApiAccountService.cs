using Axantum.AxCrypt.Api;
using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Service
{
    /// <summary>
    /// Provide basic account services using the AxCrypt API
    /// </summary>
    public class ApiAccountService : IAccountService
    {
        private AxCryptApiClient _apiClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiAccountService"/> class.
        /// </summary>
        /// <param name="apiClient">The API client to use.</param>
        public ApiAccountService(AxCryptApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        /// <summary>
        /// Gets a value indicating whether this instance has any at all accounts.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has accounts; otherwise, <c>false</c>.
        /// </value>
        public bool HasAccounts
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Lists all UserKeyPairs available for the user.
        /// </summary>
        /// <returns></returns>
        public IList<UserKeyPair> List()
        {
            return new UserKeyPair[0];
        }

        /// <summary>
        /// Saves the specified key pairs.
        /// </summary>
        /// <param name="keyPairs">The key pairs.</param>
        public void Save(IEnumerable<UserKeyPair> keyPairs)
        {
        }

        /// <summary>
        /// Changes the password for the account.
        /// </summary>
        /// <param name="passphrase">The password.</param>
        /// <returns>
        /// true if the password was successfully changed.
        /// </returns>
        public bool ChangePassphrase(Passphrase passphrase)
        {
            return false;
        }

        /// <summary>
        /// Gets the status of the account.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public AccountStatus Status
        {
            get
            {
                return AccountStatus.Offline;
            }
        }

        /// <summary>
        /// Gets the subscription level.
        /// </summary>
        /// <value>
        /// The level.
        /// </value>
        public SubscriptionLevel Level
        {
            get
            {
                return SubscriptionLevel.Unknown;
            }
        }

        /// <summary>
        /// Gets the identity this instance works with.
        /// </summary>
        /// <value>
        /// The identity.
        /// </value>
        public LogOnIdentity Identity
        {
            get
            {
                return new LogOnIdentity(EmailAddress.Parse(_apiClient.Identity.User), new Passphrase(_apiClient.Identity.Password));
            }
        }
    }
}
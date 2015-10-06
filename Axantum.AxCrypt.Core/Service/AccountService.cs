using Axantum.AxCrypt.Api;
using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Service
{
    /// <summary>
    /// Provide basic account services using the AxCrypt API
    /// </summary>
    public class AccountService
    {
        private AxCryptApiClient _apiClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountService"/> class.
        /// </summary>
        /// <param name="apiClient">The API client to use.</param>
        public AccountService(AxCryptApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        /// <summary>
        /// Lists all UserKeyPairs available for the user.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<UserKeyPair> List()
        {
            return new UserKeyPair[0];
        }

        public void Save(IEnumerable<UserKeyPair> keyPairs)
        {
        }

        public AccountStatus Status
        {
            get
            {
                return AccountStatus.Offline;
            }
        }

        public SubscriptionLevel Level
        {
            get
            {
                return SubscriptionLevel.Unknown;
            }
        }
    }
}
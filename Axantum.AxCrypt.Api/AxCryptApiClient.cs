using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Abstractions.Rest;
using Axantum.AxCrypt.Api.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Api
{
    public class AxCryptApiClient
    {
        private RestIdentity _identity;

        private Uri _baseUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="AxCryptApiClient"/> class.
        /// </summary>
        /// <param name="identity">The identity on whos behalf to make the call.</param>
        public AxCryptApiClient(RestIdentity identity, Uri baseUrl)
        {
            _identity = identity;
            _baseUrl = baseUrl;
        }

        /// <summary>
        /// Get a user summary, typically as an initial call to validate the passphrase with the account etc.
        /// </summary>
        /// <param name="email">The user name/email</param>
        /// <returns>The user summary</returns>
        public UserSummary User()
        {
            Uri resource = _baseUrl.PathCombine("api/summary");

            RestResponse response = TypeMap.Resolve.New<IRestCaller>().Send(_identity, new RestRequest(resource));
            UserSummary summary = TypeMap.Resolve.New<IStringSerializer>().Deserialize<UserSummary>(response.Content);

            return summary;
        }

        /// <summary>
        /// Checks for the most current version of AxCrypt 2.
        /// </summary>
        /// <returns>The current version information</returns>
        public CurrentVersion CheckVersion()
        {
            Uri resource = _baseUrl.PathCombine("axcrypt2version/windows");

            RestResponse response = TypeMap.Resolve.New<IRestCaller>().Send(_identity, new RestRequest(resource));
            CurrentVersion currentVersion = TypeMap.Resolve.New<IStringSerializer>().Deserialize<CurrentVersion>(response.Content);

            return currentVersion;
        }
    }
}
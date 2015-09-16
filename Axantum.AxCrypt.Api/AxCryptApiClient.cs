using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Api
{
    public class AxCryptApiClient
    {
        private LogOnIdentity _identity;

        private Uri _baseUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="AxCryptApiClient"/> class.
        /// </summary>
        /// <param name="identity">The identity on whos behalf to make the call.</param>
        public AxCryptApiClient(LogOnIdentity identity, Uri baseUrl)
        {
            _identity = identity;
            _baseUrl = baseUrl;
        }

        /// <summary>
        /// Get a user summary, typically as an initial call to validate the passphrase with the account etc.
        /// </summary>
        /// <param name="email">The user name/email</param>
        /// <returns></returns>
        public UserSummary User(EmailAddress email)
        {
            Uri resource = _baseUrl.PathCombine("/api/summary/{0}".InvariantFormat(email.Address.UrlEncode()));

            WebCallerResponse answer = Resolve.WebCaller.Send(_identity, new WebCallerRequest(resource));
            UserSummary summary = Resolve.Serializer.Deserialize<UserSummary>(answer.Content);
            return summary;
        }
    }
}
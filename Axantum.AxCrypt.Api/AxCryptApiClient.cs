using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Crypto;
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
        /// <summary>
        /// Get a user summary, typically as an initial call to validate the passphrase with the account etc.
        /// </summary>
        /// <param name="identity">The identity on whos behalf to make the call.</param>
        /// <param name="email">The user name/email</param>
        /// <returns></returns>
        public virtual UserSummary User(LogOnIdentity identity, EmailAddress email)
        {
            WebCallerResponse answer = Resolve.WebCaller.Send(identity, new WebCallerRequest(new Uri(String.Empty)));
            UserSummary summary = Resolve.Serializer.Deserialize<UserSummary>(answer.Content);
            return summary;
        }
    }
}
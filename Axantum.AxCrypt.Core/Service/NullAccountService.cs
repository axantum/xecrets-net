using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Core.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Service
{
    public class NullAccountService : IAccountService
    {
        public NullAccountService(LogOnIdentity identity)
        {
            Identity = identity;
        }

        public bool HasAccounts
        {
            get
            {
                return false;
            }
        }

        public LogOnIdentity Identity
        {
            get; private set;
        }

        public SubscriptionLevel Level
        {
            get
            {
                return SubscriptionLevel.Unknown;
            }
        }

        public AccountStatus Status
        {
            get
            {
                return AccountStatus.Unknown;
            }
        }

        public bool ChangePassphrase(Passphrase passphrase)
        {
            Identity = new LogOnIdentity(Identity.UserEmail, passphrase);
            return true;
        }

        public IList<UserKeyPair> List()
        {
            return new UserKeyPair[0];
        }

        public void Save(IEnumerable<UserKeyPair> keyPairs)
        {
        }
    }
}
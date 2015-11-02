using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Service
{
    public class CachingAccountService : IAccountService
    {
        private static readonly TimeSpan ItemExpiration = new TimeSpan(0, 5, 0);

        private class CacheKey : ICacheKey
        {
            private string _key;

            public CacheKey(string key)
            {
                _key = key;
            }

            public CacheKey SubKey(string key)
            {
                return new CacheKey(Key + "-" + key);
            }

            public string Key
            {
                get
                {
                    return _key;
                }
            }

            public TimeSpan Expiration { get { return ItemExpiration; } }
        }

        private IAccountService _service;

        private CacheKey _key;

        public CachingAccountService(IAccountService service)
        {
            _service = service;
            _key = new CacheKey(nameof(CachingAccountService)).SubKey(service.Identity.UserEmail.Address);
        }

        public bool HasAccounts
        {
            get
            {
                return New<ICache>().Get(_key.SubKey(nameof(HasAccounts)), () => _service.HasAccounts);
            }
        }

        public LogOnIdentity Identity
        {
            get
            {
                return _service.Identity;
            }
        }

        public SubscriptionLevel Level
        {
            get
            {
                return New<ICache>().Get(_key.SubKey(nameof(Level)), () => _service.Level);
            }
        }

        public async Task<UserAccount> AccountAsync()
        {
            return await New<ICache>().GetAsync(_key.SubKey(nameof(AccountAsync)), () => _service.AccountAsync()).Free();
        }

        public bool ChangePassphrase(Passphrase passphrase)
        {
            return New<ICache>().Get(_key.SubKey(nameof(ChangePassphrase)), () => _service.ChangePassphrase(passphrase));
        }

        public async Task<IList<UserKeyPair>> ListAsync()
        {
            return await New<ICache>().GetAsync(_key.SubKey(nameof(ListAsync)), () => _service.ListAsync()).Free();
        }

        public async Task PasswordResetAsync(string verificationCode)
        {
            await _service.PasswordResetAsync(verificationCode).Free();
        }

        public async Task SaveAsync(IEnumerable<UserKeyPair> keyPairs)
        {
            await New<ICache>().UpdateAsync(() => _service.SaveAsync(keyPairs), _key.SubKey(nameof(ListAsync)), _key.SubKey(nameof(AccountAsync)), _key.SubKey(nameof(HasAccounts))).Free();
        }

        public async Task SignupAsync(string emailAddress)
        {
            await _service.SignupAsync(emailAddress).Free();
        }

        public async Task<AccountStatus> StatusAsync()
        {
            return await New<ICache>().GetAsync(_key.SubKey(nameof(StatusAsync)), () => _service.StatusAsync()).Free();
        }
    }
}
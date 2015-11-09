using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
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
        private IAccountService _service;

        private CacheKey _key;

        public CachingAccountService(IAccountService service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            _service = service;
            _key = CacheKey.RootKey.Subkey(nameof(CachingAccountService)).Subkey(service.Identity.UserEmail.Address);
        }

        public bool HasAccounts
        {
            get
            {
                return New<ICache>().GetItem(_key.Subkey(nameof(HasAccounts)), () => _service.HasAccounts);
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
                return New<ICache>().GetItem(_key.Subkey(nameof(Level)), () => _service.Level);
            }
        }

        public bool ChangePassphrase(Passphrase passphrase)
        {
            bool result = false;
            New<ICache>().UpdateItem(() => result = _service.ChangePassphrase(passphrase), _key);
            return result;
        }

        public async Task<IList<UserKeyPair>> ListAsync()
        {
            return await New<ICache>().GetItemAsync(_key.Subkey(nameof(ListAsync)), () => _service.ListAsync()).Free();
        }

        public async Task<UserKeyPair> CurrentKeyPairAsync()
        {
            return await New<ICache>().GetItemAsync(_key.Subkey(nameof(CurrentKeyPairAsync)), () => _service.CurrentKeyPairAsync()).Free();
        }

        public async Task PasswordResetAsync(string verificationCode)
        {
            await _service.PasswordResetAsync(verificationCode).Free();
        }

        public async Task SaveAsync(IEnumerable<UserKeyPair> keyPairs)
        {
            await New<ICache>().UpdateItemAsync(() => _service.SaveAsync(keyPairs), _key).Free();
        }

        public async Task SignupAsync(string emailAddress)
        {
            await New<ICache>().UpdateItemAsync(() => _service.SignupAsync(emailAddress), _key);
        }

        public async Task<AccountStatus> StatusAsync()
        {
            AccountStatus status = await New<ICache>().GetItemAsync(_key.Subkey(nameof(StatusAsync)), () => _service.StatusAsync()).Free();
            if (status == AccountStatus.Offline || status == AccountStatus.Unknown)
            {
                New<ICache>().RemoveItem(_key);
            }
            return status;
        }

        public async Task<UserPublicKey> CurrentPublicKeyAsync()
        {
            return await New<ICache>().GetItemAsync(_key.Subkey(nameof(CurrentPublicKeyAsync)), () => _service.CurrentPublicKeyAsync()).Free();
        }
    }
}
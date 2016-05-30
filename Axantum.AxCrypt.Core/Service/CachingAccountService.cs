﻿using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
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
            _key = CacheKey.RootKey.Subkey(nameof(CachingAccountService)).Subkey(service.Identity.UserEmail.Address).Subkey(service.Identity.GetHashCode().ToString(CultureInfo.InvariantCulture));
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

        public async Task<bool> IsIdentityValidAsync()
        {
            return await _service.IsIdentityValidAsync().Free();
        }

        public async Task<SubscriptionLevel> LevelAsync()
        {
            return await New<ICache>().GetItemAsync(_key.Subkey(nameof(LevelAsync)), async () => await _service.LevelAsync()).Free();
        }

        public async Task<bool> ChangePassphraseAsync(Passphrase passphrase)
        {
            return await New<ICache>().UpdateItemAsync(async () => await _service.ChangePassphraseAsync(passphrase), _key).Free();
        }

        /// <summary>
        /// Fetches the user user account.
        /// </summary>
        /// <returns>
        /// The complete user account information.
        /// </returns>
        public async Task<UserAccount> AccountAsync()
        {
            return await New<ICache>().GetItemAsync(_key.Subkey(nameof(AccountAsync)), async () => await _service.AccountAsync()).Free();
        }

        public async Task<IList<UserKeyPair>> ListAsync()
        {
            return await New<ICache>().GetItemAsync(_key.Subkey(nameof(ListAsync)), async () => await _service.ListAsync()).Free();
        }

        public async Task<UserKeyPair> CurrentKeyPairAsync()
        {
            return await New<ICache>().GetItemAsync(_key.Subkey(nameof(CurrentKeyPairAsync)), async () => await _service.CurrentKeyPairAsync()).Free();
        }

        public async Task PasswordResetAsync(string verificationCode)
        {
            await _service.PasswordResetAsync(verificationCode).Free();
        }

        public async Task SaveAsync(UserAccount account)
        {
            await New<ICache>().UpdateItemAsync(async () => await _service.SaveAsync(account), _key).Free();
        }

        public async Task SaveAsync(IEnumerable<UserKeyPair> keyPairs)
        {
            await New<ICache>().UpdateItemAsync(async () => await _service.SaveAsync(keyPairs), _key).Free();
        }

        public async Task SignupAsync(EmailAddress email)
        {
            await New<ICache>().UpdateItemAsync(async () => await _service.SignupAsync(email), _key).Free();
        }

        public async Task<AccountStatus> StatusAsync(EmailAddress email)
        {
            AccountStatus status = await New<ICache>().GetItemAsync(_key.Subkey(email.Address).Subkey(nameof(StatusAsync)), async () => await _service.StatusAsync(email)).Free();
            if (status == AccountStatus.Offline || status == AccountStatus.Unknown)
            {
                New<ICache>().RemoveItem(_key);
            }
            return status;
        }

        public async Task<UserPublicKey> OtherPublicKeyAsync(EmailAddress email)
        {
            return await New<ICache>().GetItemAsync(_key.Subkey(email.Address).Subkey(nameof(OtherPublicKeyAsync)), async () => await _service.OtherPublicKeyAsync(email)).Free();
        }
    }
}
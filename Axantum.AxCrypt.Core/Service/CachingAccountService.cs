using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
            _key = CacheKey.RootKey.Subkey(nameof(CachingAccountService)).Subkey(service.Identity.UserEmail.Address).Subkey(service.Identity.Tag.ToString());
        }

        public IAccountService Refresh()
        {
            New<ICache>().RemoveItem(_key);
            return this;
        }

        public Task<bool> HasAccountsAsync()
        {
            return New<ICache>().GetItemAsync(_key.Subkey(nameof(HasAccountsAsync)), () => _service.HasAccountsAsync());
        }

        public LogOnIdentity Identity
        {
            get
            {
                return _service.Identity;
            }
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
            await New<ICache>().UpdateItemAsync(() => _service.PasswordResetAsync(verificationCode), _key).Free();
        }

        public async Task SaveAsync(UserAccount account)
        {
            await New<ICache>().UpdateItemAsync(async () => await _service.SaveAsync(account), _key).Free();
        }

        public async Task SaveAsync(IEnumerable<UserKeyPair> keyPairs)
        {
            await New<ICache>().UpdateItemAsync(async () => await _service.SaveAsync(keyPairs), _key).Free();
        }

        public async Task SignupAsync(EmailAddress email, CultureInfo culture)
        {
            await New<ICache>().UpdateItemAsync(async () => await _service.SignupAsync(email, culture), _key).Free();
        }

        public async Task<AccountStatus> StatusAsync(EmailAddress email)
        {
            AccountStatus status = await New<ICache>().UpdateItemAsync(() => _service.StatusAsync(email)).Free();
            if (status == AccountStatus.Offline || status == AccountStatus.Unknown)
            {
                New<ICache>().RemoveItem(_key);
            }
            return status;
        }

        public async Task<Offers> OffersAsync()
        {
            Offers offers = await New<ICache>().GetItemAsync(_key.Subkey(nameof(OffersAsync)), async () => await _service.OffersAsync()).Free();
            return offers;
        }

        public async Task StartPremiumTrialAsync()
        {
            await New<ICache>().UpdateItemAsync(async () => await _service.StartPremiumTrialAsync(), _key).Free();
        }

        public async Task<UserPublicKey> OtherPublicKeyAsync(EmailAddress email)
        {
            return await New<ICache>().GetItemAsync(_key.Subkey(email.Address).Subkey(nameof(OtherPublicKeyAsync)), async () => await _service.OtherPublicKeyAsync(email)).Free();
        }

        public async Task<UserPublicKey> OtherUserInvitePublicKeyAsync(EmailAddress email, CustomMessageParameters customParameters)
        {
            return await New<ICache>().GetItemAsync(_key.Subkey(email.Address).Subkey(nameof(OtherUserInvitePublicKeyAsync)), async () => await _service.OtherUserInvitePublicKeyAsync(email, customParameters)).Free();
        }

        public async Task SendFeedbackAsync(string subject, string message)
        {
            await New<ICache>().UpdateItemAsync(async () => await _service.SendFeedbackAsync(subject, message), _key).Free();
        }

        public async Task CreatePremiumAsync(StoreKitTransaction skTransaction)
        {
            await New<ICache>().UpdateItemAsync(() => _service.CreatePremiumAsync(skTransaction), _key).Free();
        }
    }
}
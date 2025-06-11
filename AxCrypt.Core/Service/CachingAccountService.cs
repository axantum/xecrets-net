using AxCrypt.Abstractions;
using AxCrypt.Api.Model;
using AxCrypt.Api.Model.Groups;
using AxCrypt.Common;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Service
{
    public class CachingAccountService : IAccountService
    {
        private readonly IAccountService _service;

        private readonly CacheKey _key;

        public CachingAccountService(IAccountService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
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

        public async Task<UserKeyPair?> CurrentKeyPairAsync()
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
            if (status is AccountStatus.Offline or AccountStatus.Unknown)
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

        public async Task<UserPublicKey?> OtherPublicKeyAsync(EmailAddress email)
        {
            return await New<ICache>().GetItemAsync(_key.Subkey(email.Address).Subkey(nameof(OtherPublicKeyAsync)), async () => await _service.OtherPublicKeyAsync(email)).Free();
        }

        public async Task<UserPublicKey?> OtherUserInvitePublicKeyAsync(EmailAddress email, CustomMessageParameters? customParameters)
        {
            return await New<ICache>().GetItemAsync(_key.Subkey(email.Address).Subkey(nameof(OtherUserInvitePublicKeyAsync)), async () => await _service.OtherUserInvitePublicKeyAsync(email, customParameters)).Free();
        }

        public async Task SendFeedbackAsync(string subject, string message)
        {
            await New<ICache>().UpdateItemAsync(async () => await _service.SendFeedbackAsync(subject, message), _key).Free();
        }

        public async Task<bool> CreateSubscriptionAsync(StoreKitTransaction[] skTransactions)
        {
            return await New<ICache>().UpdateItemAsync(() => _service.CreateSubscriptionAsync(skTransactions), _key).Free();
        }

        public async Task<PurchaseSettings?> GetInAppPurchaseSettingsAsync()
        {
            return await New<ICache>().UpdateItemAsync(() => _service.GetInAppPurchaseSettingsAsync(), _key).Free();
        }

        public async Task<bool> AutoRenewalStatusAsync()
        {
            return await New<ICache>().UpdateItemAsync(() => _service.AutoRenewalStatusAsync(), _key).Free();
        }

        public async Task<bool> DeleteUserAsync()
        {
            return await New<ICache>().UpdateItemAsync(() => _service.DeleteUserAsync(), _key).Free();
        }

        public async Task<IEnumerable<GroupKeyPairApiModel>> ListMembershipGroupsAsync()
        {
            return await _service.ListMembershipGroupsAsync();
        }
    }
}

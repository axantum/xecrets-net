#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
 *
 * This file is part of AxCrypt.
 *
 * AxCrypt is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * AxCrypt is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AxCrypt.  If not, see <http://www.gnu.org/licenses/>.
 *
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Api;
using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.Service
{
    public class ApiAccountService : IAccountService
    {
        private AxCryptApiClient _apiClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiAccountService"/> class.
        /// </summary>
        /// <param name="apiClient">The API client to use.</param>
        public ApiAccountService(AxCryptApiClient apiClient)
        {
            if (apiClient == null)
            {
                throw new ArgumentNullException(nameof(apiClient));
            }

            _apiClient = apiClient;
        }

        public IAccountService Refresh()
        {
            return this;
        }

        /// <summary>
        /// Gets a value indicating whether this instance has any at all accounts.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has accounts; otherwise, <c>false</c>.
        /// </value>
        public Task<bool> HasAccountsAsync()
        {
            if (String.IsNullOrEmpty(_apiClient.Identity.User))
            {
                throw new InvalidOperationException("The account service requires a user.");
            }

            return Task.FromResult(true);
        }

        /// <summary>
        /// Gets the identity this instance works with.
        /// </summary>
        /// <value>
        /// The identity.
        /// </value>
        public LogOnIdentity Identity
        {
            get
            {
                return new LogOnIdentity(EmailAddress.Parse(_apiClient.Identity.User), Passphrase.Create(_apiClient.Identity.Password));
            }
        }

        /// <summary>
        /// Gets the status of the account.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public async Task<AccountStatus> StatusAsync(EmailAddress email)
        {
            if (String.IsNullOrEmpty(email.Address))
            {
                return AccountStatus.Unknown;
            }

            UserAccount userAccount = await _apiClient.GetAllAccountsUserAccountAsync(email.Address).Free();
            return userAccount.AccountStatus;
        }

        /// <summary>
        /// Gets the offers used by the account.
        /// </summary>
        /// <returns>A combination of flags for offers used.</returns>
        public async Task<Offers> OffersAsync()
        {
            UserAccount userAccount = await _apiClient.MyAccountAsync().Free();
            return userAccount.Offers;
        }

        /// <summary>
        /// Start a trial period for the user.
        /// </summary>
        /// <returns></returns>
        public async Task StartPremiumTrialAsync()
        {
            await _apiClient.PostMyAccountPremiumTrial().Free();
        }

        /// <summary>
        /// Changes the password for the account.
        /// </summary>
        /// <param name="passphrase">The password.</param>
        /// <returns>
        /// true if the password was successfully changed.
        /// </returns>
        public async Task<bool> ChangePassphraseAsync(Passphrase passphrase)
        {
            if (String.IsNullOrEmpty(_apiClient.Identity.User))
            {
                throw new InvalidOperationException("The account service requires a user.");
            }

            await _apiClient.PutMyPasswordAsync(passphrase.Text).Free();
            return true;
        }

        /// <summary>
        /// Fetches the user account.
        /// </summary>
        /// <returns>
        /// The complete user account information.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">The account service requires a user.</exception>
        public async Task<UserAccount> AccountAsync()
        {
            if (string.IsNullOrEmpty(_apiClient.Identity.User))
            {
                throw new InvalidOperationException("The account service requires a user.");
            }

            try
            {
                UserAccount userAccount = await _apiClient.MyAccountAsync().Free();
                userAccount.AccountSource = AccountSource.Remote;
                return userAccount;
            }
            catch (UnauthorizedException)
            {
                return new UserAccount(Identity.UserEmail.Address);
            }
        }

        /// <summary>
        /// Lists all UserKeyPairs available for the user with the provided Identity, if any. This may filter out
        /// key pairs stored in the account, but where the private key is not decryptable with Identity.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">The account service requires a user.</exception>
        /// <exception cref="PasswordException">Credentials are not valid for server access.</exception>
        public async Task<IList<UserKeyPair>> ListAsync()
        {
            if (string.IsNullOrEmpty(_apiClient.Identity.User))
            {
                throw new InvalidOperationException("The account service requires a user.");
            }

            try
            {
                IList<AccountKey> apiAccountKeys = (await _apiClient.MyAccountAsync().Free()).AccountKeys;
                return apiAccountKeys.Select(k => k.ToUserKeyPair(Identity.Passphrase)).ToList();
            }
            catch (UnauthorizedException uaex)
            {
                throw new PasswordException("Credentials are not valid for server access.", uaex);
            }
        }

        public async Task<UserKeyPair> CurrentKeyPairAsync()
        {
            if (string.IsNullOrEmpty(_apiClient.Identity.User))
            {
                throw new InvalidOperationException("The account service requires a user.");
            }

            try
            {
                AccountKey accountKey = await _apiClient.MyAccountKeysCurrentAsync().Free();
                if (accountKey == null)
                {
                    return null;
                }
                return accountKey.ToUserKeyPair(Identity.Passphrase);
            }
            catch (UnauthorizedException uaex)
            {
                throw new PasswordException("Credentials are not valid for server access.", uaex);
            }
        }

        public async Task SaveAsync(UserAccount account)
        {
            if (string.IsNullOrEmpty(_apiClient.Identity.User))
            {
                throw new InvalidOperationException("The account service requires a user.");
            }

            try
            {
                await _apiClient.PutMyAccountAsync(account).Free();
            }
            catch (UnauthorizedException uaex)
            {
                throw new PasswordException("Credentials are not valid for server access.", uaex);
            }
        }

        /// <summary>
        /// Saves the specified key pairs.
        /// </summary>
        /// <param name="keyPairs">The key pairs.</param>
        public async Task SaveAsync(IEnumerable<UserKeyPair> keyPairs)
        {
            if (string.IsNullOrEmpty(_apiClient.Identity.User))
            {
                throw new InvalidOperationException("The account service requires a user.");
            }

            try
            {
                IList<AccountKey> apiAccountKeys = keyPairs.Select(k => k.ToAccountKey(Identity.Passphrase)).ToList();
                await _apiClient.PutMyAccountKeysAsync(apiAccountKeys).Free();
            }
            catch (UnauthorizedException uaex)
            {
                throw new PasswordException("Credentials are not valid for server access.", uaex);
            }
        }

        public async Task SendFeedbackAsync(string subject, string message)
        {
            await _apiClient.PostFeedbackAsync(subject, message).Free();
        }

        public async Task SignupAsync(EmailAddress email, CultureInfo culture)
        {
            await _apiClient.PostAllAccountsUserAsync(email.Address, culture).Free();
        }

        public async Task PasswordResetAsync(string verificationCode)
        {
            if (string.IsNullOrEmpty(_apiClient.Identity.User))
            {
                throw new InvalidOperationException("The account service requires a user.");
            }

            await _apiClient.PutAllAccountsUserPasswordAsync(verificationCode);
        }

        public async Task<UserPublicKey> OtherPublicKeyAsync(EmailAddress email)
        {
            return (await _apiClient.GetAllAccountsOtherUserPublicKeyAsync(email.Address).Free()).ToUserPublicKey();
        }

        public async Task<UserPublicKey> OtherUserInvitePublicKeyAsync(EmailAddress email, CustomMessageParameters customParameters)
        {
            return (await _apiClient.PostAllAccountsOtherUserInvitePublicKeyAsync(email.Address, customParameters).Free()).ToUserPublicKey();
        }

        public async Task CreateSubscriptionAsync(StoreKitTransaction skTransaction)
        {
            if (string.IsNullOrEmpty(_apiClient.Identity.User))
            {
                throw new InvalidOperationException("The account service requires a user.");
            }

            await _apiClient.PostCreateSubscriptionAsync(skTransaction);
        }

        public async Task<InAppPurchaseSettings> GetInAppPurchaseSettingsAsync()
        {
            if (string.IsNullOrEmpty(_apiClient.Identity.User))
            {
                throw new InvalidOperationException("The account service requires a user.");
            }

            return await _apiClient.GetInAppPurchaSettingsAsync();
        }

        public async Task<bool> AutoRenewalStatusAsync()
        {
            if (string.IsNullOrEmpty(_apiClient.Identity.User))
            {
                throw new InvalidOperationException("The account service requires a user.");
            }

            return await _apiClient.GetAutoRenewalStatusAsync();
        }
    }
}
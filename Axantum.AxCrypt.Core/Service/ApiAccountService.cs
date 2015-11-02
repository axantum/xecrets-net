#region Coypright and License

/*
 * AxCrypt - Copyright 2015, Svante Seleborg, All Rights Reserved
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
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Api;
using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            _apiClient = apiClient;
        }

        /// <summary>
        /// Gets a value indicating whether this instance has any at all accounts.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has accounts; otherwise, <c>false</c>.
        /// </value>
        public bool HasAccounts
        {
            get
            {
                return true;
            }
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
                return new LogOnIdentity(EmailAddress.Parse(_apiClient.Identity.User), new Passphrase(_apiClient.Identity.Password));
            }
        }

        /// <summary>
        /// Gets the subscription level.
        /// </summary>
        /// <value>
        /// The level.
        /// </value>
        public SubscriptionLevel Level
        {
            get
            {
                return SubscriptionLevel.Unknown;
            }
        }

        /// <summary>
        /// Gets the full account of the user this instance works with.
        /// </summary>
        /// <returns>
        /// The account.
        /// </returns>
        public Task<UserAccount> AccountAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the status of the account.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public async Task<AccountStatus> StatusAsync()
        {
            if (String.IsNullOrEmpty(Resolve.UserSettings.UserEmail))
            {
                return AccountStatus.Unknown;
            }

            UserAccount userAccount = await _apiClient.GetUserAccountAsync(Resolve.UserSettings.UserEmail).Free();
            return userAccount.AccountStatus;
        }

        /// <summary>
        /// Changes the password for the account.
        /// </summary>
        /// <param name="passphrase">The password.</param>
        /// <returns>
        /// true if the password was successfully changed.
        /// </returns>
        public bool ChangePassphrase(Passphrase passphrase)
        {
            return false;
        }

        /// <summary>
        /// Lists all UserKeyPairs available for the user.
        /// </summary>
        /// <returns></returns>
        public async Task<IList<UserKeyPair>> ListAsync()
        {
            IList<AccountKey> apiAccountKeys;
            try
            {
                apiAccountKeys = await _apiClient.AccountKeysAsync().Free();
            }
            catch (UnauthorizedApiException)
            {
                return new UserKeyPair[0];
            }
            return apiAccountKeys.Select(k => k.ToUserAsymmetricKeys(Identity.Passphrase)).ToList();
        }

        /// <summary>
        /// Saves the specified key pairs.
        /// </summary>
        /// <param name="keyPairs">The key pairs.</param>
        public async Task SaveAsync(IEnumerable<UserKeyPair> keyPairs)
        {
            IList<AccountKey> apiAccountKeys = keyPairs.Select(k => k.ToAccountKey(Identity.Passphrase)).ToList();
            await _apiClient.SaveAsync(apiAccountKeys).Free();
        }

        public async Task SignupAsync(string emailAddress)
        {
            await _apiClient.Signup(emailAddress).Free();
        }

        public async Task PasswordResetAsync(string verificationCode)
        {
            await _apiClient.VerifyAccountAsync(verificationCode);
        }
    }
}
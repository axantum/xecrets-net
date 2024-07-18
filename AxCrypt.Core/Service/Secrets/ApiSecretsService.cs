using System.Linq;
using System.Collections.Generic;

#region Coypright and License

/*
 * AxCrypt - Copyright 2023, All Rights Reserved
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
 * The source is maintained at https://bitbucket.org/axcryptab/axcrypt-net-git please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Api;
using AxCrypt.Api.Model;
using AxCrypt.Api.Model.Secret;
using AxCrypt.Common;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Extensions;
using AxCrypt.Core.Runtime;
using AxCrypt.Core.UI;
using System;
using System.Threading.Tasks;
using AxCrypt.Core.Crypto.Asymmetric;

namespace AxCrypt.Core.Service.Secrets
{
    public class ApiSecretsService : ISecretsService
    {
        private AxSecretsApiClient _apiClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiSecretsService"/> class.
        /// </summary>
        /// <param name="apiClient">The API client to use.</param>
        public ApiSecretsService(AxSecretsApiClient apiClient)
        {
            if (apiClient == null)
            {
                throw new ArgumentNullException(nameof(apiClient));
            }

            _apiClient = apiClient;
        }

        public ISecretsService Refresh()
        {
            return this;
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
        /// Fetches the encrypted user secrets.
        /// </summary>
        /// <returns>
        /// The encrypted user secrets.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">The account service requires a user.</exception>
        public async Task<EncryptedSecretApiModel> GetSecretsAsync()
        {
            if (string.IsNullOrEmpty(_apiClient.Identity.User))
            {
                throw new InvalidOperationException("The account service requires a user.");
            }

            try
            {
                EncryptedSecretApiModel userSecrets = await _apiClient.GetSecretsAsync().Free();
                return userSecrets;
            }
            catch (UnauthorizedException)
            {
                return EncryptedSecretApiModel.Empty;
            }
        }

        /// <summary>
        /// Saves the specified key pairs.
        /// </summary>
        /// <param name="secretsApiModel">The encrypted secrets.</param>
        public async Task<bool> SaveSecretsAsync(EncryptedSecretApiModel secretsApiModel)
        {
            if (string.IsNullOrEmpty(_apiClient.Identity.User))
            {
                throw new InvalidOperationException("The account service requires a user.");
            }

            try
            {
                return await _apiClient.PostSaveSecretsAsync(secretsApiModel).Free();
            }
            catch (UnauthorizedException uaex)
            {
                throw new PasswordException("Credentials are not valid for server access.", uaex);
            }
        }

        public async Task<IEnumerable<ShareSecretApiModel>> GetSharedWithSecretsAsync(SecretsListRequestOptions requestOptions)
        {
            if (string.IsNullOrEmpty(_apiClient.Identity.User))
            {
                throw new InvalidOperationException("The account service requires a user.");
            }

            try
            {
                IEnumerable<ShareSecretApiModel> userSecrets = await _apiClient.GetSharedSecretsAsync(requestOptions);
                return userSecrets;
            }
            catch (UnauthorizedException)
            {
                return new List<ShareSecretApiModel>();
            }
        }

        public async Task<UserPublicKey> OtherPublicKeyAsync(EmailAddress email)
        {
            return (await _apiClient.GetAllAccountsOtherUserPublicKeyAsync(email.Address).Free()).ToUserPublicKey();
        }

        public async Task<bool> ShareSecretsAsync(ShareSecretApiModel shareSecretApiModel)
        {
            if (string.IsNullOrEmpty(_apiClient.Identity.User))
            {
                throw new InvalidOperationException("The account service requires a user.");
            }

            try
            {
                return await _apiClient.PostShareSecretsAsync(shareSecretApiModel).Free();
            }
            catch (UnauthorizedException uaex)
            {
                throw new PasswordException("Credentials are not valid for server access.", uaex);
            }
        }

        public async Task<bool> UpdateSecretSharedWithAsync(ShareSecretApiModel shareSecretApiModel)
        {
            if (string.IsNullOrEmpty(_apiClient.Identity.User))
            {
                throw new InvalidOperationException("The account service requires a user.");
            }

            try
            {
                return await _apiClient.PostUpdateSecretSharedWithAsync(shareSecretApiModel).Free();
            }
            catch (UnauthorizedException uaex)
            {
                throw new PasswordException("Credentials are not valid for server access.", uaex);
            }
        }

        public async Task<bool> DeleteSecretSharedAsync(ShareSecretApiModel sharedSecret)
        {
            if (string.IsNullOrEmpty(_apiClient.Identity.User))
            {
                throw new InvalidOperationException("The account service requires a user.");
            }

            try
            {
                return await _apiClient.PostDeleteSharedWithAsync(sharedSecret).Free();
            }
            catch (UnauthorizedException uaex)
            {
                throw new PasswordException("Credentials are not valid for server access.", uaex);
            }
        }

        public async Task<PasswordSuggestion> SuggestPasswordAsync(int level)
        {
            if (string.IsNullOrEmpty(_apiClient.Identity.User))
            {
                throw new InvalidOperationException("The account service requires a user.");
            }

            try
            {
                PasswordSuggestion suggestedPassword = await _apiClient.SuggestStrongPasswordAsync(level).Free();
                return suggestedPassword;
            }
            catch (UnauthorizedException)
            {
                return null;
            }
        }

        public async Task<long> GetFreeUserSecretsCount(string userEmail)
        {
            if (string.IsNullOrEmpty(_apiClient.Identity.User))
            {
                throw new InvalidOperationException("The account service requires a user.");
            }
            try
            {
                return await _apiClient.GetFreeUserSecretsAsync(userEmail).Free();
            }
            catch (UnauthorizedException uaex)
            {
                throw new PasswordException("Credentials are not valid for server access.", uaex);
            }
        }

        public async Task<bool> InsertFreeUserSecretsAsync(string userEmail)
        {
            if (string.IsNullOrEmpty(_apiClient.Identity.User))
            {
                throw new InvalidOperationException("The account service requires a user.");
            }
            try
            {
                return await _apiClient.InsertFreeUserSecretsAsync(userEmail).Free();
            }
            catch (UnauthorizedException uaex)
            {
                throw new PasswordException("Credentials are not valid for server access.", uaex);
            }
        }
    }
}
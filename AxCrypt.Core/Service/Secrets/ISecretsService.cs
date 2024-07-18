using AxCrypt.Api;
using AxCrypt.Api.Model;
using AxCrypt.Api.Model.Secret;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.UI;
using System.Collections.Generic;
using System.Threading.Tasks;

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
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

namespace AxCrypt.Core.Service.Secrets
{
    /// <summary>
    /// The account service. Methods and properties to work with an account.
    /// </summary>
    public interface ISecretsService
    {
        /// <summary>
        /// Refresh all values by ensuring flushing any caches etc.
        /// </summary>
        ISecretsService Refresh();

        /// <summary>
        /// Gets the identity this instance works with.
        /// </summary>
        /// <value>
        /// The identity.
        /// </value>
        LogOnIdentity Identity { get; }

        /// <summary>
        /// Gets the secrets used by the account.
        /// </summary>
        /// <returns>A combination of flags for offers used.</returns>
        Task<EncryptedSecretApiModel> GetSecretsAsync();

        Task<bool> SaveSecretsAsync(EncryptedSecretApiModel secrets);

        Task<IEnumerable<ShareSecretApiModel>> GetSharedWithSecretsAsync(SecretsListRequestOptions requestOptions = null!);

        /// <summary>
        /// Ensures there is at least one key pair if possible, and returns the active public key of the user.
        /// </summary>
        /// <returns>The public key of the current key pair, or null if the service can't create other users.</returns>
        Task<UserPublicKey> OtherPublicKeyAsync(EmailAddress email);

        Task<bool> ShareSecretsAsync(ShareSecretApiModel sharedSecret);

        Task<bool> UpdateSecretSharedWithAsync(ShareSecretApiModel sharedSecret);

        Task<bool> DeleteSecretSharedAsync(ShareSecretApiModel sharedSecret);

        Task<PasswordSuggestion> SuggestPasswordAsync(int level);

        Task<long> GetFreeUserSecretsCount(string userEmail);

        Task<bool> InsertFreeUserSecretsAsync(string userEmail);
    }
}

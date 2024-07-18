using System;
using System.Linq;
using System.Collections.Generic;

#region Coypright and License

/*
 * AxCrypt AB - Copyright 2023, All Rights Reserved
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

using AxCrypt.Api;
using AxCrypt.Api.Model;
using AxCrypt.Api.Model.Secret;
using AxCrypt.Core.Crypto;
using System.Threading.Tasks;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.UI;

namespace AxCrypt.Core.Service.Secrets
{
    public class NullSecretsService : ISecretsService
    {
        private static readonly Task<bool> _completedTask = Task.FromResult(true);

        public NullSecretsService(LogOnIdentity identity)
        {
            Identity = identity;
        }

        public ISecretsService Refresh()
        {
            return this;
        }

        public LogOnIdentity Identity
        {
            get; private set;
        }

        public Task<EncryptedSecretApiModel> GetSecretsAsync()
        {
            return Task.FromResult(EncryptedSecretApiModel.Empty);
        }

        public Task<bool> SaveSecretsAsync(EncryptedSecretApiModel secrets)
        {
            return _completedTask;
        }

        public Task<IEnumerable<ShareSecretApiModel>> GetSharedWithSecretsAsync(SecretsListRequestOptions requestOptions)
        {
            IEnumerable<ShareSecretApiModel> shareSecretList = new List<ShareSecretApiModel>();
            return Task.FromResult(shareSecretList as IEnumerable<ShareSecretApiModel>);
        }

        public Task<UserPublicKey> OtherPublicKeyAsync(EmailAddress email)
        {
            return Task.FromResult((UserPublicKey)null!);
        }

        public Task<bool> ShareSecretsAsync(ShareSecretApiModel secrets)
        {
            return _completedTask;
        }

        public Task<bool> UpdateSecretSharedWithAsync(ShareSecretApiModel secrets)
        {
            return _completedTask;
        }

        public Task<bool> DeleteSecretSharedAsync(ShareSecretApiModel secrets)
        {
            return _completedTask;
        }

        public Task<PasswordSuggestion> SuggestPasswordAsync(int level)
        {
            return null!;
        }

        public Task<long> GetFreeUserSecretsCount(string userEmail)
        {
            throw new NotImplementedException();
        }

        public Task<bool> InsertFreeUserSecretsAsync(string userEmail)
        {
            throw new NotImplementedException();
        }
    }
}

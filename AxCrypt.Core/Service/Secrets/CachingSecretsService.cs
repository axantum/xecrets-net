using AxCrypt.Abstractions;
using AxCrypt.Api;
using AxCrypt.Api.Model;
using AxCrypt.Api.Model.Secret;
using AxCrypt.Common;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Service.Secrets
{
    public class CachingSecretsService : ISecretsService
    {
        private ISecretsService _service;

        private CacheKey _key;

        public CachingSecretsService(ISecretsService service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            _service = service;
            _key = CacheKey.RootKey.Subkey(nameof(CachingSecretsService)).Subkey(service.Identity.UserEmail.Address).Subkey(service.Identity.Tag.ToString());
        }

        public ISecretsService Refresh()
        {
            New<ICache>().RemoveItem(_key);
            return this;
        }

        public LogOnIdentity Identity
        {
            get
            {
                return _service.Identity;
            }
        }

        public async Task<EncryptedSecretApiModel> GetSecretsAsync()
        {
            return await New<ICache>().UpdateItemAsync(async () => await _service.GetSecretsAsync()).Free();
        }

        public async Task<bool> SaveSecretsAsync(EncryptedSecretApiModel secretsApiModel)
        {
            return await New<ICache>().UpdateItemAsync(async () => await _service.SaveSecretsAsync(secretsApiModel), _key).Free();
        }

        public async Task<IEnumerable<ShareSecretApiModel>> GetSharedWithSecretsAsync(SecretsListRequestOptions requestOptions)
        {
            return await New<ICache>().UpdateItemAsync(async () => await _service.GetSharedWithSecretsAsync(requestOptions)).Free();
        }

        public async Task<UserPublicKey> OtherPublicKeyAsync(EmailAddress email)
        {
            return await New<ICache>().GetItemAsync(_key.Subkey(email.Address).Subkey(nameof(OtherPublicKeyAsync)), async () => await _service.OtherPublicKeyAsync(email)).Free();
        }

        public async Task<bool> ShareSecretsAsync(ShareSecretApiModel secretsApiModel)
        {
            return await New<ICache>().UpdateItemAsync(async () => await _service.ShareSecretsAsync(secretsApiModel), _key).Free();
        }

        public async Task<bool> UpdateSecretSharedWithAsync(ShareSecretApiModel sharedSecret)
        {
            return await New<ICache>().UpdateItemAsync(async () => await _service.UpdateSecretSharedWithAsync(sharedSecret), _key).Free();
        }

        public async Task<bool> DeleteSecretSharedAsync(ShareSecretApiModel sharedSecret)
        {
            return await New<ICache>().UpdateItemAsync(async () => await _service.DeleteSecretSharedAsync(sharedSecret), _key).Free();
        }

        public async Task<PasswordSuggestion> SuggestPasswordAsync(int level)
        {
            return await New<ICache>().UpdateItemAsync(async () => await _service.SuggestPasswordAsync(level), _key).Free();
        }

        public async Task<long> GetFreeUserSecretsCount(string userEmail)
        {
            return await New<ICache>().UpdateItemAsync(async () => await _service.GetFreeUserSecretsCount(userEmail), _key).Free();
        }

        public async Task<bool> InsertFreeUserSecretsAsync(string userEmail)
        {
            return await New<ICache>().UpdateItemAsync(async () => await _service.InsertFreeUserSecretsAsync(userEmail), _key).Free();
        }
    }
}
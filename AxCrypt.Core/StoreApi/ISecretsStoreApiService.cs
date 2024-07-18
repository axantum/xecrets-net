using AxCrypt.Api;
using AxCrypt.Api.Model.Secret;
using AxCrypt.Api.Model.User;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AxCrypt.Core.StoreApi
{
    public interface ISecretsStoreApiService
    {
        Task<bool> MigrateUserAsync(MigrateApiModel migrateApiModel);

        Task<bool> SaveAsync(SecretsApiModel secret);

        Task<bool> ShareSecretAsync(ShareSecretApiModel secret);

        Task<SecretsApiModel> GetAsync(SecretsListRequestOptions requestOptions);

        Task<IEnumerable<ShareSecretApiModel>> GetSharedListAsync(SecretsListRequestOptions requestOptions);

        Task<bool> UpdateSecretSharedWithAsync(ShareSecretApiModel sharedSecret);

        Task<bool> DeleteSecretSharedAsync(ShareSecretApiModel sharedSecret);
    }
}
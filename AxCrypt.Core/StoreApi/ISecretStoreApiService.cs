using AxCrypt.Api.Model.Secret;
using AxCrypt.Api.Model.User;
using AxCrypt.Core.Secrets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Core.StoreApi
{
    public interface ISecretStoreApiService
    {
        Task<IList<SecretApiModel>> GetListAsync(string userEmail);
    }
}
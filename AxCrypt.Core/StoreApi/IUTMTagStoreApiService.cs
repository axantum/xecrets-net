using AxCrypt.Api.Model.UTM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Core.StoreApi
{
    public interface IUTMTagStoreApiService
    {
        Task<Guid> CreateAsync(UTMTagApiModel model);

        Task<IList<UTMTagApiModel>> GetUTMTagsAsync();

        Task<bool> UpdateAsync(UTMTagApiModel model);

        Task<bool> DeleteAsync(Guid id);

        Task<UTMTagApiModel> GetAsync(Guid id);
    }
}
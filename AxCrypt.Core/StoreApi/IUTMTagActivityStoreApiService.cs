using AxCrypt.Api.Model.UTM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Core.StoreApi
{
    public interface IUTMTagActivityStoreApiService
    {
        Task<bool> CreateAsync(UTMTagActivityApiModel utm);

        Task<bool> CreateAsync(UTMTagUserActivityApiModel utm);

        Task<bool> CreateAsync(UTMTagPurchaseActivityApiModel utm);

        Task<IEnumerable<UTMTagActivityExportApiModel>> ListUTMTagActivitiesAsync(IEnumerable<Guid> id);
    }
}
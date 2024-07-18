using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AxCrypt.Core.StoreApi
{
    public interface IStatsStoreApiService
    {
        Task<AxCrypt.Abstractions.Rest.RestResponse> GetBusSubsStatsAsync(string email);

        Task<AxCrypt.Abstractions.Rest.RestResponse> GetBusinessMRRStatsAsync(string userEmail);

        Task<bool> RefreshStatsDataAsync(string userEmail);
    }
}
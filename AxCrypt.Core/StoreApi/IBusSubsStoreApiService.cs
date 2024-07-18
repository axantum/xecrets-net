using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AxCrypt.Core.StoreApi
{
    public interface IBusSubsStoreApiService
    {
        Task<AxCrypt.Abstractions.Rest.RestResponse> GetAsync(string busSubsId);

        Task<bool> SaveAsync(AxCrypt.Abstractions.Rest.RestContent restContent);

        Task<bool> RenewAsync(AxCrypt.Abstractions.Rest.RestContent restContent);

        Task<bool> TopupAsync(AxCrypt.Abstractions.Rest.RestContent restContent);

        Task<bool> UpdateSubscriptionStatusAsync(string busSubsId, AxCrypt.Abstractions.Rest.RestContent restContent);

        Task<bool> UpdateMasterKeyStatusAsync(string busSubsId, AxCrypt.Abstractions.Rest.RestContent restContent);

        Task<bool> UpdateResellerStatusAsync(string busSubsId, AxCrypt.Abstractions.Rest.RestContent restContent);

        Task<AxCrypt.Abstractions.Rest.RestResponse> GetBusSubsInfoAsync(string busSubsId);

        Task<bool> UpdateBusSubsInfoAsync(string busSubsId, AxCrypt.Abstractions.Rest.RestContent restContent);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AxCrypt.Core.StoreApi
{
    public interface IBusSubsIndexStoreApiService
    {
        Task<AxCrypt.Abstractions.Rest.RestResponse> GetAllBusinessMembershipsAsync(string userEmail);

        Task<AxCrypt.Abstractions.Rest.RestResponse> GetUserMembershipSubscriptionsAsync(string userEmail, bool allPayments);

        Task<bool> SaveAsync(AxCrypt.Abstractions.Rest.RestContent restContent);

        Task<bool> DeleteMemberAsync(string userEmail, string subscriptionId);

        Task<bool> DeleteMembershipsAsync(string userEmail);

        Task<bool> ChangeMembershipsAsync(string fromUser, string toUser);

        Task<AxCrypt.Abstractions.Rest.RestResponse> LoadMembershipsWithNameAsync(string userEmail);

        Task<AxCrypt.Abstractions.Rest.RestResponse> ResellerMembershipsAsync(string userEmail);

        Task<AxCrypt.Abstractions.Rest.RestResponse> GetBusSubsStats(AxCrypt.Abstractions.Rest.RestContent restContent, string email);
    }
}
using AxCrypt.Abstractions.Rest;
using AxCrypt.Api.Model.PrivateSubscription;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AxCrypt.Core.StoreApi
{
    public interface IPrivateSubscriptionStoreApiService
    {
        Task<bool> CreateAsync(PrivateSubscriptionInformationApiModel privateSubscriptionMetaDataApiModel);

        Task<bool> SiteAdminCancelSubsAsync(string adminEmial, PrivateSubscriptionInformationApiModel privateSubscriptionMetaDataApiModel);

        Task<bool> CopyUserSubsAsync(string fromUser, string toUser);

        Task<bool> MoveUserSubsAsync(string fromUser, string toUser);

        Task<IEnumerable<PrivateSubscriptionApiModel>> GetSubsEventAsync(string email);

        Task<IEnumerable<PrivateSubscriptionInformationApiModel>> GetAllSubscriptionEventsAsync(string email, string subscriptionFilter);

        Task<bool> MigrateXecretsServiceAsync(RestContent restContent);

        Task<SubscriptionBaseApiModel> GetEffectiveSubscriptionAsync(string userEmail);
    }
}
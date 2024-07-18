using AxCrypt.Api.Model.PushNotification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AxCrypt.Core.StoreApi
{
    public interface IPushNotificationStoreApiService
    {
        Task<IEnumerable<PushNotificationApiModel>> GetListAsync(string userEmail);

        Task<bool> CreateAsync(PushNotificationApiModel model);

        Task<PushNotificationApiModel> GetAsync(long id);

        Task<bool> UpdateAsync(PushNotificationApiModel model);

        Task<bool> DispatchAsync(PushNotificationDispatchApiModel model);

        Task<bool> DeleteAsync(long id);
    }
}
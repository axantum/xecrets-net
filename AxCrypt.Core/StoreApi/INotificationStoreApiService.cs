using AxCrypt.Api.Model.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AxCrypt.Core.StoreApi
{
    public interface INotificationStoreApiService
    {
        Task<bool> CreateAsync(IEnumerable<NotificationApiModel> model);

        Task<IEnumerable<UserNotificationApiModel>> GetListAsync(string userEmail, string subsLevel);

        Task<bool> DeleteAsync(long id);
    }
}
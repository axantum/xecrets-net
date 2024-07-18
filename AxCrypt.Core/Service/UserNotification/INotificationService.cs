using AxCrypt.Api.Model.Notification;
using AxCrypt.Core.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AxCrypt.Core.Service.UserNotification
{
    public interface INotificationService
    {
        INotificationService Refresh();

        LogOnIdentity? Identity { get; }

        Task<IEnumerable<UserNotificationApiModel>> GetAllUserNotificationAsync(string useremail, string subslevel);
    }
}

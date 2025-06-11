using AxCrypt.Abstractions;
using AxCrypt.Api.Model.Notification;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.IO;
using AxCrypt.Core.Service.UserNotification;
using AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Service.Secrets
{
    public class LocalNotificationService : INotificationService
    {
        public LocalNotificationService()
        {

        }

        public INotificationService Refresh()
        {
            return this;
        }

        public LogOnIdentity? Identity
        {
            get;
        }

        public Task<IEnumerable<UserNotificationApiModel>> GetAllUserNotificationAsync(string useremail, string subslevel)
        {
            return Task.FromResult((IEnumerable<UserNotificationApiModel>)new List<UserNotificationApiModel>());
        }

        public Task<bool> InsertUserNotificationAsync(IEnumerable<NotificationApiModel> notificationModel)
        {
            return Task.FromResult(false);
        }
    }
}

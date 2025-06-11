using AxCrypt.Api.Model.Notification;
using AxCrypt.Core.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AxCrypt.Core.Service.UserNotification
{
    public class NullNotificationService : INotificationService
    {
        private static readonly Task<bool> _completedTask = Task.FromResult(true);

        public NullNotificationService(LogOnIdentity identity)
        {
            Identity = identity;
        }

        public INotificationService Refresh()
        {
            return this;
        }

        public LogOnIdentity Identity
        {
            get; private set;
        }

        public Task<IEnumerable<UserNotificationApiModel>> GetAllUserNotificationAsync(string useremail, string subslevel)
        {
            return Task.FromResult<IEnumerable<UserNotificationApiModel>>([]);
        }

        public Task<bool> InsertUserNotificationAsync(IEnumerable<NotificationApiModel> notificationModel)
        {
            return Task.FromResult(false);
        }
    }
}

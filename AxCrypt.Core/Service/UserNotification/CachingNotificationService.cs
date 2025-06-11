using AxCrypt.Abstractions;
using AxCrypt.Api.Model.Notification;
using AxCrypt.Common;
using AxCrypt.Core.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Service.UserNotification
{
    public class CachingNotificationService : INotificationService
    {
        private INotificationService _service;

        private CacheKey _key;

        public CachingNotificationService(INotificationService service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            _service = service;
            _key = CacheKey.RootKey.Subkey(nameof(CachingNotificationService)).Subkey(service.Identity!.UserEmail.Address).Subkey(service.Identity.Tag.ToString());
        }

        public LogOnIdentity Identity => throw new NotImplementedException();

        public async Task<IEnumerable<UserNotificationApiModel>> GetAllUserNotificationAsync(string useremail, string subslevel)
        {
            return await New<ICache>().GetItemAsync(_key.Subkey(nameof(GetAllUserNotificationAsync)), async () => await _service.GetAllUserNotificationAsync(useremail, subslevel)).Free();
        }

        public async Task<bool> InsertUserNotificationAsync(IEnumerable<NotificationApiModel> notificationModel)
        {
            return await New<ICache>().UpdateItemAsync(async () => await _service.InsertUserNotificationAsync(notificationModel), _key).Free();
        }

        public INotificationService Refresh()
        {
            New<ICache>().RemoveItem(_key);
            return this;
        }
    }
}

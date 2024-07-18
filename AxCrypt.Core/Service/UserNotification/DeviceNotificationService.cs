using AxCrypt.Api.Model.Notification;
using AxCrypt.Common;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Service.UserNotification
{
    public class DeviceNotificationService : INotificationService
    {
        private INotificationService _localService;
        private INotificationService _remoteService;

        public DeviceNotificationService(INotificationService localService, INotificationService remoteService)
        {
            _localService = localService;
            _remoteService = remoteService;
        }

        public INotificationService Refresh()
        {
            return this;
        }

        public LogOnIdentity Identity
        {
            get
            {
                return _remoteService.Identity!;
            }
        }

        public async Task<IEnumerable<UserNotificationApiModel>> GetAllUserNotificationAsync(string useremail, string subslevel)
        {
            if (New<AxCryptOnlineState>().IsOnline && Identity != LogOnIdentity.Empty)
            {
                try
                {
                    return await _remoteService.GetAllUserNotificationAsync(useremail, subslevel).Free();
                }
                catch (ApiException aex)
                {
                    await aex.HandleApiExceptionAsync();
                }
            }

            return await _localService.GetAllUserNotificationAsync(useremail, subslevel).Free();
        }
    }
}

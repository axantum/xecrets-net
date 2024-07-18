using System.Linq;
using System.Collections.Generic;

#region Coypright and License

/*
 * AxCrypt - Copyright 2023, All Rights Reserved
 *
 * This file is part of AxCrypt.
 *
 * AxCrypt is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * AxCrypt is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AxCrypt.  If not, see <http://www.gnu.org/licenses/>.
 *
 * The source is maintained at https://bitbucket.org/axcryptab/axcrypt-net-git please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Api;
using AxCrypt.Common;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Service.UserNotification;
using AxCrypt.Core.UI;
using System;
using System.Threading.Tasks;
using AxCrypt.Api.Model.Notification;

namespace AxCrypt.Core.Service.Secrets
{
    public class ApiNotificationService : INotificationService
    {
        private AxNotificationApiClient _apiClient;

        public ApiNotificationService(AxNotificationApiClient apiClient)
        {
            if (apiClient == null)
            {
                throw new ArgumentNullException(nameof(apiClient));
            }

            _apiClient = apiClient;
        }

        public INotificationService Refresh()
        {
            return this;
        }

        public LogOnIdentity Identity
        {
            get
            {
                return new LogOnIdentity(EmailAddress.Parse(_apiClient.Identity.User), Passphrase.Create(_apiClient.Identity.Password));
            }
        }

        public async Task<IEnumerable<UserNotificationApiModel>> GetAllUserNotificationAsync(string useremail, string subslevel)
        {
            if (string.IsNullOrEmpty(_apiClient.Identity.User))
            {
                throw new InvalidOperationException("The account service requires a user.");
            }

            try
            {
                IEnumerable<UserNotificationApiModel> notificationList = await _apiClient.GetAllUserNotificationAsync(useremail, subslevel).Free();
                return notificationList;
            }
            catch (UnauthorizedException ex)
            {
                throw ex;
            }
        }
    }
}
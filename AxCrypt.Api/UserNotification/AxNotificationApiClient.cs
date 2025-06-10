using AxCrypt.Abstractions;
using AxCrypt.Abstractions.Rest;
using AxCrypt.Api.Model.Notification;
using AxCrypt.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Api
{
    /// <summary>
    /// Provide basic api services using the AxCrypt API. All connection errors are thrown as OfflineApiExceptions, which must be caught and
    /// handled by the caller, and should be treated as 'temporarily offline'. They root cause can be both Internet connection issues as well
    /// as the servers being down.
    /// </summary>
    public class AxNotificationApiClient
    {
        private Uri BaseUrl { get; }

        private TimeSpan Timeout { get; }

        private ApiCaller Caller { get; } = new ApiCaller();

        /// <summary>
        /// Initializes a new instance of the <see cref="AxNotificationApiClient"/> class.
        /// </summary>
        /// <param name="identity">The identity on whos behalf to make the call.</param>
        public AxNotificationApiClient(RestIdentity identity, Uri baseUrl, TimeSpan timeout)
        {
            Identity = identity;
            BaseUrl = baseUrl;
            Timeout = timeout;
        }

        public RestIdentity Identity { get; }

        /// <summary>
        /// Get a all user notification.
        /// </summary>
        /// <param name="email">The user name/email</param>
        /// <returns>The user notifications</returns>
        public async Task<IEnumerable<UserNotificationApiModel>> GetAllUserNotificationAsync(string useremail, string subslevel)
        {
            Uri resource = BaseUrl.PathCombine("notification/list?email={0}&subslevel={1}".With(useremail, subslevel));
            RestResponse restResponse = await Caller.RestAsync(Identity, new RestRequest("GET", resource, Timeout)).Free();
            ApiCaller.EnsureStatusOk(restResponse);

            IEnumerable<UserNotificationApiModel> notificationList = Serializer.Deserialize<IEnumerable<UserNotificationApiModel>>(restResponse.Content);

            return notificationList;
        }

        public async Task<bool> InsertUserNotificationAsync(IEnumerable<NotificationApiModel> notificationList)
        {
            if (notificationList == null && !notificationList.Any())
            {
                throw new ArgumentNullException(nameof(notificationList));
            }

            Uri resource = BaseUrl.PathCombine("notification/insert".With());

            RestContent content = new RestContent(Serializer.Serialize(notificationList));
            RestResponse restResponse = await Caller.RestAsync(Identity, new RestRequest("POST", resource, Timeout, content)).Free();
            ApiCaller.EnsureStatusOk(restResponse);

            return Serializer.Deserialize<bool>(restResponse.Content);
        }

        private static IStringSerializer Serializer
        {
            get
            {
                return New<IStringSerializer>();
            }
        }
    }
}
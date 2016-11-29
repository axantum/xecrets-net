using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Abstractions.Rest;
using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Api
{
    /// <summary>
    /// Provide basic api services using the AxCrypt API. All connection errors are thrown as OfflineApiExceptions, which must be caught and
    /// handled by the caller, and should be treated as 'temporarily offline'. They root cause can be both Internet connection issues as well
    /// as the servers being down.
    /// </summary>
    public class AxCryptApiClient
    {
        private Uri BaseUrl { get; }

        private TimeSpan Timeout { get; }

        private ApiCaller Caller { get; } = new ApiCaller();

        /// <summary>
        /// Initializes a new instance of the <see cref="AxCryptApiClient"/> class.
        /// </summary>
        /// <param name="identity">The identity on whos behalf to make the call.</param>
        public AxCryptApiClient(RestIdentity identity, Uri baseUrl, TimeSpan timeout)
        {
            Identity = identity;
            BaseUrl = baseUrl;
            Timeout = timeout;
        }

        public RestIdentity Identity { get; }

        /// <summary>
        /// Get a user summary anonymously, typically as an initial call to check the status of the account etc.
        /// </summary>
        /// <param name="email">The user name/email</param>
        /// <returns>The user summary</returns>
        public async Task<UserAccount> GetAllAccountsUserAccountAsync(string userName)
        {
            if (userName == null)
            {
                throw new ArgumentNullException(nameof(userName));
            }

            Uri resource = BaseUrl.PathCombine("users/all/accounts/{0}".With(ApiCaller.UrlEncode(userName)));

            RestResponse restResponse = await Caller.RestAsync(new RestIdentity(), new RestRequest(resource, Timeout)).Free();
            if (restResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return new UserAccount(userName, SubscriptionLevel.Unknown, AccountStatus.NotFound, Offers.None);
            }
            if (restResponse.StatusCode == HttpStatusCode.BadRequest)
            {
                return new UserAccount(userName, SubscriptionLevel.Unknown, AccountStatus.InvalidName, Offers.None);
            }
            ApiCaller.EnsureStatusOk(restResponse);

            UserAccount userAccount = Serializer.Deserialize<UserAccount>(restResponse.Content);
            return userAccount;
        }

        /// <summary>
        /// Gets the user account with the provided credentials.
        /// </summary>
        /// <returns>All account information for the user.</returns>
        /// <exception cref="System.InvalidOperationException">There must be an identity and password to attempt to get private account information.</exception>
        public async Task<UserAccount> MyAccountAsync()
        {
            if (string.IsNullOrEmpty(Identity.User) || string.IsNullOrEmpty(Identity.Password))
            {
                throw new InvalidOperationException("There must be an identity and password to attempt to get private account information.");
            }

            Uri resource = BaseUrl.PathCombine("users/my/account");

            RestResponse restResponse = await Caller.RestAsync(Identity, new RestRequest(resource, Timeout)).Free();
            ApiCaller.EnsureStatusOk(restResponse);

            UserAccount userAccount = Serializer.Deserialize<UserAccount>(restResponse.Content);
            return userAccount;
        }

        public async Task<AccountKey> MyAccountKeysCurrentAsync()
        {
            Uri resource = BaseUrl.PathCombine("users/my/account/keys/current");

            RestResponse restResponse = await Caller.RestAsync(Identity, new RestRequest(resource, Timeout)).Free();
            if (restResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            ApiCaller.EnsureStatusOk(restResponse);

            AccountKey accountKey = Serializer.Deserialize<AccountKey>(restResponse.Content);
            return accountKey;
        }

        public async Task PutMyAccountAsync(UserAccount account)
        {
            Uri resource = BaseUrl.PathCombine("users/my/account".With(ApiCaller.UrlEncode(Identity.User)));

            RestContent content = new RestContent(Serializer.Serialize(account));
            RestResponse restResponse = await Caller.RestAsync(Identity, new RestRequest("PUT", resource, Timeout, content)).Free();
            ApiCaller.EnsureStatusOk(restResponse);
        }

        /// <summary>
        /// Uploads a key pair to server. The operation is idempotent.
        /// </summary>
        /// <param name="accountKeys">The account keys to upload.</param>
        public async Task PutMyAccountKeysAsync(IEnumerable<AccountKey> accountKeys)
        {
            Uri resource = BaseUrl.PathCombine("users/my/account/keys".With(ApiCaller.UrlEncode(Identity.User)));

            RestContent content = new RestContent(Serializer.Serialize(accountKeys));
            RestResponse restResponse = await Caller.RestAsync(Identity, new RestRequest("PUT", resource, Timeout, content)).Free();
            ApiCaller.EnsureStatusOk(restResponse);
        }

        /// <summary>
        /// Start a Premium Trial period for the user.
        /// </summary>
        /// <returns></returns>
        public async Task PostMyAccountPremiumTrial()
        {
            Uri resource = BaseUrl.PathCombine("users/my/account/premiumtrial".With(ApiCaller.UrlEncode(Identity.User)));

            RestResponse restResponse = await Caller.RestAsync(Identity, new RestRequest("POST", resource, Timeout));
            ApiCaller.EnsureStatusOk(restResponse);
        }

        /// <summary>
        /// Changes the password of the user.
        /// </summary>
        /// <param name="newPassword">The new password.</param>
        /// <returns></returns>
        public async Task PutMyPasswordAsync(string newPassword)
        {
            Uri resource = BaseUrl.PathCombine("users/my/account/password");

            PasswordResetParameters passwordResetParameters = new PasswordResetParameters(newPassword, string.Empty);
            RestContent content = new RestContent(Serializer.Serialize(passwordResetParameters));
            RestResponse restResponse = await Caller.RestAsync(Identity, new RestRequest("PUT", resource, Timeout, content)).Free();
            ApiCaller.EnsureStatusOk(restResponse);
        }

        /// <summary>
        /// Gets the public key of any user. If the user does not exist, he or she is invited by the current user.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        public async Task<AccountKey> GetAllAccountsOtherUserPublicKeyAsync(string userName)
        {
            if (userName == null)
            {
                throw new ArgumentNullException(nameof(userName));
            }

            Uri resource = BaseUrl.PathCombine("users/all/accounts/{0}/key".With(ApiCaller.UrlEncode(userName)));

            RestResponse restResponse = await Caller.RestAsync(Identity, new RestRequest(resource, Timeout)).Free();
            ApiCaller.EnsureStatusOk(restResponse);

            AccountKey accountKey = Serializer.Deserialize<AccountKey>(restResponse.Content);
            return accountKey;
        }

        /// <summary>
        /// Checks for the most current version of AxCrypt 2.
        /// </summary>
        /// <returns>The current version information</returns>
        public async Task<CurrentVersionResponse> CheckVersionAsync(string currentVersion)
        {
            if (currentVersion == null)
            {
                throw new ArgumentNullException("currentVersion");
            }

            Uri resource = BaseUrl.PathCombine("axcrypt2version/windows?current={0}".With(ApiCaller.UrlEncode(currentVersion)));

            RestResponse restResponse = await Caller.RestAsync(Identity, new RestRequest(resource, Timeout)).Free();
            ApiCaller.EnsureStatusOk(restResponse);

            CurrentVersionResponse apiResponse = Serializer.Deserialize<CurrentVersionResponse>(restResponse.Content);
            ApiCaller.EnsureStatusOk(apiResponse);

            return apiResponse;
        }

        public async Task PostAllAccountsUserAsync(string userName, CultureInfo culture)
        {
            if (userName == null)
            {
                throw new ArgumentNullException(nameof(userName));
            }
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            Uri resource = BaseUrl.PathCombine("users/all/accounts/{0}?culture={1}".With(ApiCaller.UrlEncode(userName), culture.Name));

            RestResponse restResponse = await Caller.RestAsync(new RestIdentity(), new RestRequest("POST", resource, Timeout)).Free();
            ApiCaller.EnsureStatusOk(restResponse);
        }

        public async Task PutAllAccountsUserPasswordAsync(string verification)
        {
            if (string.IsNullOrEmpty(Identity.User) || string.IsNullOrEmpty(Identity.Password))
            {
                throw new InvalidOperationException("There must be an identity and password to attempt to verify the account information.");
            }

            Uri resource = BaseUrl.PathCombine("users/all/accounts/{0}/password".With(Identity.User));

            PasswordResetParameters passwordResetParameters = new PasswordResetParameters(Identity.Password, verification);
            RestContent content = new RestContent(Serializer.Serialize(passwordResetParameters));
            RestResponse restResponse = await Caller.RestAsync(Identity, new RestRequest("PUT", resource, Timeout, content)).Free();
            ApiCaller.EnsureStatusOk(restResponse);
        }

        public async Task PostFeedbackAsync(string subject, string message)
        {
            if (subject == null)
            {
                throw new ArgumentNullException(nameof(subject));
            }
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            if (message.Length == 0)
            {
                throw new ArgumentException("Message must contain something.", nameof(message));
            }

            Uri resource = BaseUrl.PathCombine("users/my/account/feedback");

            FeedbackData feedbackData = new FeedbackData(subject, message);
            RestContent content = new RestContent(Serializer.Serialize(feedbackData));
            RestResponse restResponse = await Caller.RestAsync(Identity, new RestRequest("POST", resource, Timeout, content)).Free();
            ApiCaller.EnsureStatusOk(restResponse);
        }

        public async Task<AxCryptVersion> AxCryptUpdateAsync(Version currentVersion, string cultureName)
        {
            Uri resource;
            if (Identity.IsEmpty)
            {
                resource = BaseUrl.PathCombine($"global/axcrypt/version/windowsdesktop?version={currentVersion?.ToString() ?? string.Empty}");
            }
            else
            {
                resource = BaseUrl.PathCombine($"users/axcrypt/version/windowsdesktop?version={currentVersion?.ToString() ?? string.Empty}&culture={cultureName}");
            }

            if (New<AxCryptOnlineState>().IsOffline)
            {
                return AxCryptVersion.Empty;
            }

            try
            {
                RestResponse restResponse = await Caller.RestAsync(Identity, new RestRequest(resource, Timeout)).Free();
                ApiCaller.EnsureStatusOk(restResponse);
                AxCryptVersion axCryptVersion = Serializer.Deserialize<AxCryptVersion>(restResponse.Content);
                return axCryptVersion;
            }
            catch (OfflineApiException oaex)
            {
                New<IReport>().Exception(oaex);
                New<AxCryptOnlineState>().IsOffline = true;
            }
            return AxCryptVersion.Empty;
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
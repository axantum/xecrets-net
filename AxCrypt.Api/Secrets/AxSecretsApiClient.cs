using AxCrypt.Abstractions;
using AxCrypt.Abstractions.Rest;
using AxCrypt.Api.Model;
using AxCrypt.Api.Model.Secret;
using AxCrypt.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Api
{
    /// <summary>
    /// Provide basic api services using the AxCrypt API. All connection errors are thrown as OfflineApiExceptions, which must be caught and
    /// handled by the caller, and should be treated as 'temporarily offline'. They root cause can be both Internet connection issues as well
    /// as the servers being down.
    /// </summary>
    public class AxSecretsApiClient
    {
        private Uri BaseUrl { get; }

        private TimeSpan Timeout { get; }

        private ApiCaller Caller { get; } = new ApiCaller();

        /// <summary>
        /// Initializes a new instance of the <see cref="AxSecretsApiClient"/> class.
        /// </summary>
        /// <param name="identity">The identity on whos behalf to make the call.</param>
        public AxSecretsApiClient(RestIdentity identity, Uri baseUrl, TimeSpan timeout)
        {
            Identity = identity;
            BaseUrl = baseUrl;
            Timeout = timeout;
        }

        public RestIdentity Identity { get; }

        /// <summary>
        /// Get a user secrets.
        /// </summary>
        /// <param name="email">The user name/email</param>
        /// <returns>The user secrets</returns>
        public async Task<EncryptedSecretApiModel> GetSecretsAsync()
        {
            Uri resource = BaseUrl.PathCombine("secrets/get");

            RestResponse restResponse = await Caller.RestAsync(Identity, new RestRequest(resource, Timeout)).Free();
            ApiCaller.EnsureStatusOk(restResponse);

            EncryptedSecretApiModel userSecrets = Serializer.Deserialize<EncryptedSecretApiModel>(restResponse.Content);
            return userSecrets;
        }

        public async Task<bool> PostSaveSecretsAsync(EncryptedSecretApiModel secretsModel)
        {
            if (secretsModel == null)
            {
                throw new ArgumentNullException(nameof(secretsModel));
            }

            Uri resource = BaseUrl.PathCombine("secrets/save");
            RestContent content = new RestContent(Serializer.Serialize(secretsModel));
            RestResponse restResponse = await Caller.RestAsync(Identity, new RestRequest("POST", resource, Timeout, content)).Free();
            ApiCaller.EnsureStatusOk(restResponse);

            return Serializer.Deserialize<bool>(restResponse.Content);
        }

        public async Task<IEnumerable<ShareSecretApiModel>> GetSharedSecretsAsync(SecretsListRequestOptions requestOptions)
        {
            Uri resource = BaseUrl.PathCombine("secrets/shared/list");

            RestResponse restResponse = await Caller.RestAsync(Identity, new RestRequest(resource, Timeout)).Free();
            ApiCaller.EnsureStatusOk(restResponse);

            IEnumerable<ShareSecretApiModel> userSecrets = Serializer.Deserialize<IEnumerable<ShareSecretApiModel>>(restResponse.Content);
            return userSecrets;
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

            Uri resource = BaseUrl.PathCombine("secrets/sharesecret/{0}/key".With(ApiCaller.PathSegmentEncode(userName)));

            RestResponse restResponse = await Caller.RestAsync(Identity, new RestRequest(resource, Timeout)).Free();
            ApiCaller.EnsureStatusOk(restResponse);

            AccountKey accountKey = Serializer.Deserialize<AccountKey>(restResponse.Content);
            return accountKey;
        }

        public async Task<bool> PostShareSecretsAsync(ShareSecretApiModel shareSecretApiModel)
        {
            if (shareSecretApiModel == null)
            {
                throw new ArgumentNullException(nameof(shareSecretApiModel));
            }

            Uri resource = BaseUrl.PathCombine("secrets/share");
            RestContent content = new RestContent(Serializer.Serialize(shareSecretApiModel));

            RestResponse restResponse = await Caller.RestAsync(Identity, new RestRequest("POST", resource, Timeout, content)).Free();
            ApiCaller.EnsureStatusOk(restResponse);

            return Serializer.Deserialize<bool>(restResponse.Content);
        }

        public async Task<bool> PostUpdateSecretSharedWithAsync(ShareSecretApiModel shareSecretApiModel)
        {
            if (shareSecretApiModel == null)
            {
                throw new ArgumentNullException(nameof(shareSecretApiModel));
            }

            Uri resource = BaseUrl.PathCombine("secrets/update/sharedwith");
            RestContent content = new RestContent(Serializer.Serialize(shareSecretApiModel));

            RestResponse restResponse = await Caller.RestAsync(Identity, new RestRequest("POST", resource, Timeout, content)).Free();
            ApiCaller.EnsureStatusOk(restResponse);

            return Serializer.Deserialize<bool>(restResponse.Content);
        }

        public async Task<bool> PostDeleteSharedWithAsync(ShareSecretApiModel secretsModel)
        {
            if (secretsModel == null)
            {
                throw new ArgumentNullException(nameof(secretsModel));
            }

            Uri resource = BaseUrl.PathCombine("secrets/shared/delete");
            RestContent content = new RestContent(Serializer.Serialize(secretsModel));
            RestResponse restResponse = await Caller.RestAsync(Identity, new RestRequest("POST", resource, Timeout, content)).Free();
            ApiCaller.EnsureStatusOk(restResponse);

            return Serializer.Deserialize<bool>(restResponse.Content);
        }

        public async Task<PasswordSuggestion> SuggestStrongPasswordAsync(int level)
        {
            Uri resource = BaseUrl.PathCombine($"global/passwords/suggestion/{level}");

            RestResponse restResponse = await Caller.RestAsync(Identity, new RestRequest(resource, Timeout)).Free();
            ApiCaller.EnsureStatusOk(restResponse);

            PasswordSuggestion suggestedPassword = Serializer.Deserialize<PasswordSuggestion>(restResponse.Content);
            return suggestedPassword;
        }

        public async Task<long> GetFreeUserSecretsAsync(string userEmail)
        {
            if (userEmail == null)
            {
                throw new ArgumentNullException(nameof(userEmail));
            }

            Uri resource = BaseUrl.PathCombine("secrets/nonpayinguser/createlimit?userEmail=" + ApiCaller.PathSegmentEncode(userEmail));

            RestResponse restResponse = await Caller.RestAsync(Identity, new RestRequest(resource, Timeout)).Free();
            ApiCaller.EnsureStatusOk(restResponse);

            return Serializer.Deserialize<long>(restResponse.Content);
        }

        public async Task<bool> InsertFreeUserSecretsAsync(string userEmail)
        {
            if (userEmail == null)
            {
                throw new ArgumentNullException(nameof(userEmail));
            }

            Uri resource = BaseUrl.PathCombine("secrets/nonpayinguser/createlimit?userEmail=" + ApiCaller.PathSegmentEncode(userEmail));
            
            RestContent content = new RestContent(Serializer.Serialize(userEmail));
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
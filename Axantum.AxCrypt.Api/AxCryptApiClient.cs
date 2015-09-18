using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Abstractions.Rest;
using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Api.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Api
{
    public class AxCryptApiClient
    {
        private RestIdentity _identity;

        private Uri _baseUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="AxCryptApiClient"/> class.
        /// </summary>
        /// <param name="identity">The identity on whos behalf to make the call.</param>
        public AxCryptApiClient(RestIdentity identity, Uri baseUrl)
        {
            _identity = identity;
            _baseUrl = baseUrl;
        }

        /// <summary>
        /// Get a user summary, typically as an initial call to validate the passphrase with the account etc.
        /// </summary>
        /// <param name="email">The user name/email</param>
        /// <returns>The user summary</returns>
        public UserSummary User()
        {
            Uri resource = _baseUrl.PathCombine("summary");

            RestResponse restResponse = RestCallInternal(_identity, new RestRequest(resource));
            EnsureStatusOk(restResponse);

            SummaryResponse apiResponse = Serializer.Deserialize<SummaryResponse>(restResponse.Content);
            EnsureStatusOk(apiResponse);

            return apiResponse.Summary;
        }

        /// <summary>
        /// Fetches a users current public key. The server will always return one, auto-signing up the
        /// user if required and relevant.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>The users public key. If the caller does not have the apprpriate subscription level, and empty instance is returned.</returns>
        public UserPublicKey PublicKey(string user)
        {
            Uri resource = _baseUrl.PathCombine("publickey/{0}".With(RestCaller.UrlEncode(user)));

            RestResponse restResponse = RestCallInternal(_identity, new RestRequest(resource));
            EnsureStatusOk(restResponse);

            UserPublicKeyResponse apiResponse = Serializer.Deserialize<UserPublicKeyResponse>(restResponse.Content);
            if (apiResponse.Status == (int)CommonStatus.PaymentRequired)
            {
                return new UserPublicKey();
            }
            EnsureStatusOk(apiResponse);

            return apiResponse.PublicKey;
        }

        /// <summary>
        /// Uploads a key pair to server. The operation is idempotent.
        /// </summary>
        /// <param name="keyPairs">The key pair.</param>
        public void UploadKeyPair(KeyPair keyPair)
        {
            Uri resource = _baseUrl.PathCombine("keypair/{0}".With(RestCaller.UrlEncode(keyPair.Thumbprint)));

            RestContent content = new RestContent(Serializer.Serialize(keyPair));
            RestResponse restResponse = RestCallInternal(_identity, new RestRequest("PUT", resource, content));
            EnsureStatusOk(restResponse);

            ResponseBase apiResponse = Serializer.Deserialize<ResponseBase>(restResponse.Content);
            EnsureStatusOk(apiResponse);
        }

        /// <summary>
        /// Downloads a key pair. The download is the most recently known with the given thumbprint, if any.
        /// </summary>
        /// <param name="thumbprint">The thumbprint of the key pair to download.</param>
        /// <returns>The keypair</returns>
        public KeyPair DownloadKeyPair(string thumbprint)
        {
            Uri resource = _baseUrl.PathCombine("keypair/{0}".With(RestCaller.UrlEncode(thumbprint)));
            RestResponse restResponse = RestCallInternal(_identity, new RestRequest("GET", resource));
            EnsureStatusOk(restResponse);

            KeyPairResponse response = Serializer.Deserialize<KeyPairResponse>(restResponse.Content);
            EnsureStatusOk(response);

            return response.KeyPair;
        }

        /// <summary>
        /// Checks for the most current version of AxCrypt 2.
        /// </summary>
        /// <returns>The current version information</returns>
        public CurrentVersionResponse CheckVersion(string currentVersion)
        {
            if (currentVersion == null)
            {
                throw new ArgumentNullException("currentVersion");
            }

            Uri resource = _baseUrl.PathCombine("axcrypt2version/windows?current={0}".With(RestCaller.UrlEncode(currentVersion)));

            RestResponse restResponse = RestCallInternal(_identity, new RestRequest(resource));
            EnsureStatusOk(restResponse);

            CurrentVersionResponse apiResponse = Serializer.Deserialize<CurrentVersionResponse>(restResponse.Content);
            EnsureStatusOk(apiResponse);

            return apiResponse;
        }

        private static RestResponse RestCallInternal(RestIdentity identity, RestRequest request)
        {
            try
            {
                return RestCaller.Send(identity, request);
            }
            catch (Exception ex)
            {
                throw new ApiException("REST call failed with exception.", ex);
            }
        }

        private void EnsureStatusOk(RestResponse restResponse)
        {
            if (restResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedApiException(restResponse.Content, ErrorStatus.ApiHttpResponseError);
            }

            if (restResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new ApiException(restResponse.Content, ErrorStatus.ApiHttpResponseError);
            }
        }

        private static void EnsureStatusOk(ResponseBase apiResponse)
        {
            if (apiResponse.Status != 0)
            {
                throw new ApiException(apiResponse.Message, ErrorStatus.ApiError);
            }
        }

        private static IRestCaller RestCaller
        {
            get
            {
                return TypeMap.Resolve.New<IRestCaller>();
            }
        }

        private static IStringSerializer Serializer
        {
            get
            {
                return TypeMap.Resolve.New<IStringSerializer>();
            }
        }
    }
}
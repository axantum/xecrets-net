﻿using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Abstractions.Rest;
using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Api.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Api
{
    public class AxCryptApiClient
    {
        private Uri _baseUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="AxCryptApiClient"/> class.
        /// </summary>
        /// <param name="identity">The identity on whos behalf to make the call.</param>
        public AxCryptApiClient(RestIdentity identity, Uri baseUrl)
        {
            Identity = identity;
            _baseUrl = baseUrl;
        }

        public RestIdentity Identity { get; private set; }

        /// <summary>
        /// Get a user summary anonymously, typically as an initial call to validate the passphrase with the account etc.
        /// </summary>
        /// <param name="email">The user name/email</param>
        /// <returns>The user summary</returns>
        public UserAccount GetUserAccount(string userName)
        {
            if (userName == null)
            {
                throw new ArgumentNullException(nameof(userName));
            }

            Uri resource = _baseUrl.PathCombine("users/account/{0}".With(UrlEncode(userName)));

            RestResponse restResponse = RestCallInternal(Identity, new RestRequest(resource));
            EnsureStatusOk(restResponse);

            UserAccountResponse apiResponse = Serializer.Deserialize<UserAccountResponse>(restResponse.Content);
            EnsureStatusOk(apiResponse);

            return apiResponse.UserAccount;
        }

        /// <summary>
        /// Gets the user account with the provided credentials.
        /// </summary>
        /// <returns>All account information for the user.</returns>
        /// <exception cref="System.InvalidOperationException">There must be an identity and password to attempt to get private account information.</exception>
        public UserAccount GetUserAccount()
        {
            if (String.IsNullOrEmpty(Identity.User) || String.IsNullOrEmpty(Identity.Password))
            {
                throw new InvalidOperationException("There must be an identity and password to attempt to get private account information.");
            }

            Uri resource = _baseUrl.PathCombine("users/account");

            RestResponse restResponse = RestCallInternal(Identity, new RestRequest(resource));
            EnsureStatusOk(restResponse);

            UserAccountResponse apiResponse = Serializer.Deserialize<UserAccountResponse>(restResponse.Content);
            EnsureStatusOk(apiResponse);

            return apiResponse.UserAccount;
        }

        /// <summary>
        /// Uploads a key pair to server. The operation is idempotent.
        /// </summary>
        /// <param name="accountKeys">The account keys to upload.</param>
        public void PutAccountKeys(IEnumerable<AccountKey> accountKeys)
        {
            Uri resource = _baseUrl.PathCombine("users/{0}/account-keys".With(UrlEncode(Identity.User)));

            RestContent content = new RestContent(Serializer.Serialize(accountKeys));
            RestResponse restResponse = RestCallInternal(Identity, new RestRequest("PUT", resource, content));
            EnsureStatusOk(restResponse);

            ResponseBase apiResponse = Serializer.Deserialize<ResponseBase>(restResponse.Content);
            EnsureStatusOk(apiResponse);
        }

        /// <summary>
        /// Downloads the account keys of the user.
        /// </summary>
        /// <returns>The account keys of the account.</returns>
        public IList<AccountKey> AccountKeys()
        {
            Uri resource = _baseUrl.PathCombine("users/{0}/account-keys".With(UrlEncode(Identity.User)));
            RestResponse restResponse = RestCallInternal(Identity, new RestRequest("GET", resource));
            EnsureStatusOk(restResponse);

            AccountKeyResponse response = Serializer.Deserialize<AccountKeyResponse>(restResponse.Content);
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

            Uri resource = _baseUrl.PathCombine("axcrypt2version/windows?current={0}".With(UrlEncode(currentVersion)));

            RestResponse restResponse = RestCallInternal(Identity, new RestRequest(resource));
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

        private static void EnsureStatusOk(RestResponse restResponse)
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
                return New<IRestCaller>();
            }
        }

        private static string UrlEncode(string value)
        {
            return RestCaller.UrlEncode(value);
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
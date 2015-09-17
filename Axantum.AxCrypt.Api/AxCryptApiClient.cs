using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Abstractions.Rest;
using Axantum.AxCrypt.Api.Model;
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
            Uri resource = _baseUrl.PathCombine("api/summary");

            RestResponse restResponse = RestCaller.Send(_identity, new RestRequest(resource));
            SummaryResponse response = Serializer.Deserialize<SummaryResponse>(restResponse.Content);
            if (response.Status != 0)
            {
                throw new ApiException(response.Message, ErrorStatus.ApiError);
            }

            return response.Summary;
        }

        public void UploadKeyPairs(IEnumerable<KeyPair> keyPairs)
        {
            Uri resource = _baseUrl.PathCombine("api/keypairs");

            RestContent content = new RestContent(Serializer.Serialize(keyPairs));
            RestResponse restResponse = RestCaller.Send(_identity, new RestRequest("PUT", resource, content));
            CommonResponse response = Serializer.Deserialize<CommonResponse>(restResponse.Content);

            if (response.Status != 0)
            {
                throw new ApiException(response.Message, ErrorStatus.ApiError);
            }
        }

        /// <summary>
        /// Checks for the most current version of AxCrypt 2.
        /// </summary>
        /// <returns>The current version information</returns>
        public CurrentVersion CheckVersion()
        {
            Uri resource = _baseUrl.PathCombine("axcrypt2version/windows");

            RestResponse restResponse = RestCaller.Send(_identity, new RestRequest(resource));
            CurrentVersion response = Serializer.Deserialize<CurrentVersion>(restResponse.Content);

            if (response.Status != 0)
            {
                throw new ApiException(response.Message, ErrorStatus.ApiError);
            }

            return response;
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
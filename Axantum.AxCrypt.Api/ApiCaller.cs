using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Abstractions.Rest;
using Axantum.AxCrypt.Api.Response;
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
    public class ApiCaller
    {
        public ApiCaller()
        {
        }

        public async Task<RestResponse> RestAsync(RestIdentity identity, RestRequest request)
        {
            try
            {
                RestResponse response = await RestCaller.SendAsync(identity, request).Free();
                return response;
            }
            catch (Exception ex) when (!(ex is OfflineApiException))
            {
                throw new ApiException(string.Format(CultureInfo.InvariantCulture, "{2} {1} {0}", request.Url, request.Method, ex.Message), ex);
            }
        }

        public static void EnsureStatusOk(RestResponse restResponse)
        {
            if (restResponse == null)
            {
                throw new ArgumentNullException(nameof(restResponse));
            }

            if (restResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedApiException(restResponse.Content, ErrorStatus.ApiHttpResponseError);
            }
            if (restResponse.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                throw new OfflineApiException("Service unavailable.");
            }
            if (restResponse.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new BadRequestApiException("Malformed API request.");
            }
            if (restResponse.StatusCode != HttpStatusCode.OK && restResponse.StatusCode != HttpStatusCode.Created)
            {
                throw new ApiException(restResponse.Content, ErrorStatus.ApiHttpResponseError);
            }
        }

        public static void EnsureStatusOk(ResponseBase apiResponse)
        {
            if (apiResponse == null)
            {
                throw new ArgumentNullException(nameof(apiResponse));
            }

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
        public static string UrlEncode(string value)
        {
            return RestCaller.UrlEncode(value);
        }

        public static string PathSegmentEncode(string value)
        {
            return UrlEncode(value).Replace("%2B", "+").Replace("%40", "@");
        }
    }
}
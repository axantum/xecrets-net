using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Abstractions.Rest;
using Axantum.AxCrypt.Api.Response;
using Axantum.AxCrypt.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
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
                return await RestCaller.SendAsync(identity, request).Free();
            }
            catch (WebException wex)
            {
                throw new OfflineApiException(ExceptionMessage("Offline", request), wex);
            }
            catch (HttpRequestException hrex)
            {
                throw new OfflineApiException(ExceptionMessage("Offline", request), hrex);
            }
            catch (Exception ex)
            {
                throw new ApiException(ExceptionMessage("REST call failed", request), ex);
            }
        }

        private static string ExceptionMessage(string message, RestRequest request)
        {
            return string.Format(CultureInfo.InvariantCulture, "{2} {1} {0}", request.Url, request.Method, message);
        }

        public void EnsureStatusOk(RestResponse restResponse)
        {
            if (restResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedApiException(restResponse.Content, ErrorStatus.ApiHttpResponseError);
            }
            if (restResponse.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                throw new OfflineApiException("Service unavailable");
            }
            if (restResponse.StatusCode != HttpStatusCode.OK && restResponse.StatusCode != HttpStatusCode.Created)
            {
                throw new ApiException(restResponse.Content, ErrorStatus.ApiHttpResponseError);
            }
        }

        public void EnsureStatusOk(ResponseBase apiResponse)
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

        public string UrlEncode(string value)
        {
            return RestCaller.UrlEncode(value);
        }
    }
}
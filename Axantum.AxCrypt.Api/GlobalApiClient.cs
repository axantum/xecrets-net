using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Abstractions.Rest;
using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Common;
using System;
using System.Globalization;
using System.Threading.Tasks;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Api
{
    public class GlobalApiClient
    {
        private ApiCaller Caller { get; } = new ApiCaller();

        private Uri BaseUrl { get; }

        private TimeSpan Timeout { get; }

        private static IStringSerializer Serializer
        {
            get
            {
                return New<IStringSerializer>();
            }
        }

        public GlobalApiClient(Uri baseUrl, TimeSpan timeout)
        {
            BaseUrl = baseUrl;
            Timeout = timeout;
        }

        public async Task<ApiVersion> ApiVersionAsync(string platform, string appVersion)
        {
            Uri resource = BaseUrl.PathCombine("global/apiversion");

            RestResponse restResponse;
            if (New<AxCryptOnlineState>().IsOnline)
            {
                try
                {
                    RestRequest restRequest = new RestRequest(resource, Timeout);
                    restRequest.Headers.Collection["Platform"] = platform;
                    restRequest.Headers.Collection["App-Version"] = appVersion;
                    restRequest.Headers.Collection["App-Culture"] = CultureInfo.CurrentUICulture.DisplayName;

                    restResponse = await Caller.RestAsync(new RestIdentity(), restRequest).Free();
                    ApiCaller.EnsureStatusOk(restResponse);
                    ApiVersion apiVersion = Serializer.Deserialize<ApiVersion>(restResponse.Content);
                    return apiVersion;
                }
                catch (OfflineApiException oaex)
                {
                    New<IReport>().Exception(oaex);
                    New<AxCryptOnlineState>().IsOffline = true;
                }
                catch (Exception ex)
                {
                    New<IReport>().Exception(ex);
                }
            }
            return ApiVersion.Zero;
        }
    }
}
using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Abstractions.Rest;
using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public async Task<ApiVersion> ApiVersionAsync()
        {
            Uri resource = BaseUrl.PathCombine("global/apiversion");

            RestResponse restResponse;
            if (New<AxCryptOnlineState>().IsOnline)
            {
                try
                {
                    restResponse = await Caller.RestAsync(new RestIdentity(), new RestRequest(resource, Timeout)).Free();
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

        public async Task<AxCryptVersion> AxCryptUpdateAsync()
        {
            Uri resource = BaseUrl.PathCombine("global/axcrypt/version/windowsdesktop");
            if (New<AxCryptOnlineState>().IsOffline)
            {
                return AxCryptVersion.Empty;
            }

            try
            {
                RestResponse restResponse = await Caller.RestAsync(new RestIdentity(), new RestRequest(resource, Timeout)).Free();
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
    }
}
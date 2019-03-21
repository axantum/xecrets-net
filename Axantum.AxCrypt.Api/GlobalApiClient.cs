using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Abstractions.Rest;
using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Common;
using System;
using System.Collections.Generic;
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

        public async Task<ApiVersion> ApiVersionAsync(string appPlatform, string appVersion)
        {
            Uri resource = BaseUrl.PathCombine($"global/apiversion?AppPlatform={appPlatform ?? ""}&AppVersion={appVersion ?? ""}&UserCulture={CultureInfo.CurrentUICulture.Name}");

            RestResponse restResponse;
            if (New<AxCryptOnlineState>().IsOnline)
            {
                restResponse = await Caller.RestAsync(new RestIdentity(), new RestRequest(resource, Timeout)).Free();
                ApiCaller.EnsureStatusOk(restResponse);
                ApiVersion apiVersion = Serializer.Deserialize<ApiVersion>(restResponse.Content);
                return apiVersion;
            }
            return ApiVersion.Zero;
        }

        public async Task<IList<CultureInfo>> GetCultureInfoListAsync()
        {
            Uri resource = BaseUrl.PathCombine("global/support/cultures");

            if (New<AxCryptOnlineState>().IsOnline)
            {
                RestResponse restResponse = await Caller.RestAsync(new RestIdentity(), new RestRequest(resource, Timeout)).Free();
                ApiCaller.EnsureStatusOk(restResponse);
                return Serializer.Deserialize<IList<CultureInfo>>(restResponse.Content);
            }

            return await Task.FromResult((IList<CultureInfo>)null); ;
        }
    }
}
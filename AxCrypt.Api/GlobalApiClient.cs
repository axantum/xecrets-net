using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using AxCrypt.Abstractions;
using AxCrypt.Abstractions.Rest;
using AxCrypt.Api.Model;
using AxCrypt.Common;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Api
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
                return DeserialieWorkAroundForCultureInfoPclFrameworkProblems(restResponse);
            }

            return await Task.FromResult((IList<CultureInfo>)null); ;
        }

        private static IList<CultureInfo> DeserialieWorkAroundForCultureInfoPclFrameworkProblems(RestResponse restResponse)
        {
            IList<string> cultureInfoStrings = Serializer.Deserialize<IList<string>>(restResponse.Content);
            return cultureInfoStrings.Select(cit => new CultureInfo(cit)).ToList();
        }
    }
}
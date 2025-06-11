using System.Threading.Tasks;

namespace AxCrypt.Core.StoreApi
{
    public interface IPurchaseSettingsStoreApiService
    {
        Task<bool> UpdateAsync(AxCrypt.Abstractions.Rest.RestContent restContent);

        Task<AxCrypt.Abstractions.Rest.RestResponse> GetListAsync();
    }
}
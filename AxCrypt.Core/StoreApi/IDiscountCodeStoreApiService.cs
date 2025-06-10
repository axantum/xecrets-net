using System.Threading.Tasks;

namespace AxCrypt.Core.StoreApi
{
    public interface IDiscountCodeStoreApiService
    {
        Task<AxCrypt.Abstractions.Rest.RestResponse> GetListAsync();

        Task<bool> SaveAsync(AxCrypt.Abstractions.Rest.RestContent restContent);

        Task<bool> UpdateAsync(AxCrypt.Abstractions.Rest.RestContent restContent, bool canUpdateAutoEnable);

        Task<bool> DeleteAsync(string discountCodeName, string subscriptionLevel);
    }
}
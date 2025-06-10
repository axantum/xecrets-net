using System.Threading.Tasks;

namespace AxCrypt.Core.StoreApi
{
    public interface IPayPalPurchaseStgsStoreApiService
    {
        Task<AxCrypt.Abstractions.Rest.RestResponse> GetListAsync();

        Task<bool> SaveAsync(AxCrypt.Abstractions.Rest.RestContent restContent);

        Task<bool> UpdateAsync(AxCrypt.Abstractions.Rest.RestContent restContent);

        Task<bool> EmailChangeAsync(string oldUser, string newUser);

        Task<bool> DeleteAsync(string subscriptionId);
    }
}
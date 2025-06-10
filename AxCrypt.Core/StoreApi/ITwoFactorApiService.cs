using AxCrypt.Api.Model.User;
using System.Threading.Tasks;

namespace AxCrypt.Core.StoreApi
{
    public interface ITwoFactorApiService
    {
        Task<bool> SaveTwoFactorAuthenticationInfosync(TwoFactorAuthApiModel twoFactorAuthenticationApiModel);

        Task<TwoFactorAuthApiModel> GetTwoFactorAuthenticationInfoAsync(string userEmail);
    }
}
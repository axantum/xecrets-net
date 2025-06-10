using AxCrypt.Api.Model.User;
using System.Threading.Tasks;

namespace AxCrypt.Core.StoreApi
{
    public interface IUserStoreApiService
    {
        Task<bool> UpdateAsync(string userEmail, UserApiModel secret);

        Task<bool> DeleteAsync(string userEmail);

        Task<UserApiModel> GetTwoFactorAuthStatusAsync(string userEmail);

        Task<bool> UpdateTwoFactorStatusAsync(UserApiModel userApiModel);
    }
}
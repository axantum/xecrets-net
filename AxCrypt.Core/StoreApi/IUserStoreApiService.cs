using AxCrypt.Api.Model.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Core.StoreApi
{
    public interface IUserStoreApiService
    {
        Task<bool> UpdateAsync(string userEmail, UserApiModel secret);

        Task<bool> DeleteAsync(string userEmail);
    }
}
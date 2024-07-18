using AxCrypt.Api.Model.Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Core.StoreApi
{
    public interface IStrpFailedTransStoreApiService
    {
        Task<bool> CreateAsync(StrpFailedTransApiModel model);

        Task<bool> UpdateAsync(StrpFailedTransApiModel model);

        Task<StrpFailedTransApiModel> GetAsync(string userEmail);
    }
}
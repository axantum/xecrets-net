using AxCrypt.Api.Model;
using AxCrypt.Api.Model.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Core.StoreApi
{
    public interface ISubsCancelnStoreApiService
    {
        Task<bool> SaveAsync(SubsCancellationApiModel subsCancelnRsn);

        Task<IList<SubsCancellationApiModel>> GetListAsync(DateTime startdate, DateTime endDate);
    }
}
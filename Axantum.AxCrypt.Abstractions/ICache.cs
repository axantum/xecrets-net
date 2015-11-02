using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Abstractions
{
    public interface ICache
    {
        T Get<T>(ICacheKey cacheKey, Func<T> itemFunction);

        Task<T> GetAsync<T>(ICacheKey cacheKey, Func<Task<T>> itemFunction);

        Task UpdateAsync(Func<Task> updateFunction, params ICacheKey[] dependencies);
    }
}
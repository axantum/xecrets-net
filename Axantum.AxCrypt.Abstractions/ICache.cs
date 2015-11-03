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

        void Update(Action updateAction, params ICacheKey[] dependencies);

        Task UpdateAsync(Func<Task> updateFunction, params ICacheKey[] dependencies);

        Task<T> UpdateAsync<T>(Func<Task<T>> updateFunction, params ICacheKey[] dependencies);

        void Remove(ICacheKey cacheKey);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Abstractions
{
    public interface ICache
    {
        T GetItem<T>(ICacheKey cacheKey, Func<T> itemFunction);

        Task<T> GetItemAsync<T>(ICacheKey cacheKey, Func<Task<T>> itemFunction);

        void UpdateItem(Action updateAction, params ICacheKey[] dependencies);

        Task UpdateItemAsync(Func<Task> updateFunction, params ICacheKey[] dependencies);

        Task<T> UpdateItemAsync<T>(Func<Task<T>> updateFunction, params ICacheKey[] dependencies);

        void RemoveItem(ICacheKey cacheKey);
    }
}
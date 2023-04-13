using AxCrypt.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Fake
{
    public class FakeCache : ICache
    {
        public T GetItem<T>(ICacheKey cacheKey, Func<T> itemFunction)
        {
            return itemFunction();
        }

        public Task<T> GetItemAsync<T>(ICacheKey cacheKey, Func<Task<T>> itemFunction)
        {
            return itemFunction();
        }

        public void RemoveItem(ICacheKey cacheKey)
        {
        }

        public void UpdateItem(Action updateAction, params ICacheKey[] dependencies)
        {
        }

        public Task UpdateItemAsync(Func<Task> updateFunction, params ICacheKey[] dependencies)
        {
            return Task.CompletedTask;
        }

        public Task<T> UpdateItemAsync<T>(Func<Task<T>> updateFunction, params ICacheKey[] dependencies)
        {
            return updateFunction();
        }
    }
}

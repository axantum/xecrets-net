using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Mono
{
    public class ItemCache : ICache
    {
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public T Get<T>(ICacheKey cacheKey, Func<T> itemFunction)
        {
            _lock.Wait();
            try
            {
                object o = MemoryCache.Default.Get(cacheKey.Key);
                if (o != null)
                {
                    return (T)o;
                }
                T item = itemFunction();
                MemoryCache.Default.Add(cacheKey.Key, item, DateTime.Now.Add(cacheKey.Expiration));
                return item;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<T> GetAsync<T>(ICacheKey cacheKey, Func<Task<T>> itemFunction)
        {
            await _lock.WaitAsync().Free();
            try
            {
                object o = MemoryCache.Default.Get(cacheKey.Key);
                if (o != null)
                {
                    return (T)o;
                }
                T item = await itemFunction().Free();
                MemoryCache.Default.Add(cacheKey.Key, item, DateTime.Now.Add(cacheKey.Expiration));
                return item;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task UpdateAsync(Func<Task> updateFunction, params ICacheKey[] dependencies)
        {
            await _lock.WaitAsync().Free();
            try
            {
                await updateFunction().Free();
                foreach (ICacheKey key in dependencies)
                {
                    MemoryCache.Default.Remove(key.Key);
                }
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
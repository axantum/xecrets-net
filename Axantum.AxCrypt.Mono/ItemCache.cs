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
    public class ItemCache : ICache, IDisposable
    {
        private SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        private static readonly object _object = new object();

        public T GetItem<T>(ICacheKey cacheKey, Func<T> itemFunction)
        {
            if (cacheKey == null)
            {
                throw new ArgumentNullException(nameof(cacheKey));
            }
            if (itemFunction == null)
            {
                throw new ArgumentNullException(nameof(itemFunction));
            }

            _lock.Wait();
            try
            {
                object o = MemoryCache.Default.Get(cacheKey.Key);
                if (o != null)
                {
                    return (T)o;
                }
                T item = itemFunction();
                MemoryCache.Default.Add(cacheKey.Key, item, Policy(cacheKey));
                return item;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<T> GetItemAsync<T>(ICacheKey cacheKey, Func<Task<T>> itemFunction)
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
                MemoryCache.Default.Add(cacheKey.Key, item, Policy(cacheKey));
                return item;
            }
            finally
            {
                _lock.Release();
            }
        }

        private static CacheItemPolicy Policy(ICacheKey cacheKey)
        {
            for (ICacheKey key = cacheKey.ParentCacheKey; key != null; key = key.ParentCacheKey)
            {
                if (MemoryCache.Default.Contains(key.Key))
                {
                    continue;
                }

                CacheItemPolicy keyPolicy = new CacheItemPolicy();
                if (key.ParentCacheKey != null)
                {
                    keyPolicy.ChangeMonitors.Add(MemoryCache.Default.CreateCacheEntryChangeMonitor(new string[] { key.ParentCacheKey.Key }));
                }
                MemoryCache.Default.Add(key.Key, _object, keyPolicy);
            }

            CacheItemPolicy policy = new CacheItemPolicy();
            if (cacheKey.Expiration != TimeSpan.Zero)
            {
                policy.AbsoluteExpiration = DateTime.Now.Add(cacheKey.Expiration);
            }
            policy.ChangeMonitors.Add(MemoryCache.Default.CreateCacheEntryChangeMonitor(new string[] { cacheKey.ParentCacheKey.Key }));

            return policy;
        }

        public void UpdateItem(Action updateAction, params ICacheKey[] dependencies)
        {
            if (updateAction == null)
            {
                throw new ArgumentNullException(nameof(updateAction));
            }
            if (dependencies == null)
            {
                throw new ArgumentNullException(nameof(dependencies));
            }

            _lock.Wait();
            try
            {
                updateAction();
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

        public async Task UpdateItemAsync(Func<Task> updateFunction, params ICacheKey[] dependencies)
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

        public async Task<T> UpdateItemAsync<T>(Func<Task<T>> updateFunction, params ICacheKey[] dependencies)
        {
            await _lock.WaitAsync().Free();
            try
            {
                T item = await updateFunction().Free();
                foreach (ICacheKey key in dependencies)
                {
                    MemoryCache.Default.Remove(key.Key);
                }
                return item;
            }
            finally
            {
                _lock.Release();
            }
        }

        public void RemoveItem(ICacheKey cacheKey)
        {
            if (cacheKey == null)
            {
                throw new ArgumentNullException(nameof(cacheKey));
            }

            MemoryCache.Default.Remove(cacheKey.Key);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (_lock != null)
            {
                _lock.Dispose();
                _lock = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
using AxCrypt.Abstractions;
using AxCrypt.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AxCrypt.Desktop
{
    public class ItemCache : ICache, IDisposable
    {
        private SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        private MemoryCache _cache = new MemoryCache("ItemCache");

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
                object o = _cache.Get(cacheKey.Key);
                if (o != null)
                {
                    return (T)o;
                }
                T item = itemFunction();
                _cache.Add(cacheKey.Key, item, Policy(cacheKey));
                return item;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<T> GetItemAsync<T>(ICacheKey cacheKey, Func<Task<T>> itemFunctionAsync)
        {
            await _lock.WaitAsync().Free();
            try
            {
                object o = _cache.Get(cacheKey.Key);
                if (o != null)
                {
                    return (T)o;
                }
                T item = await itemFunctionAsync().Free();
                if (item != null)
                {
                    _cache.Add(cacheKey.Key, item, Policy(cacheKey));
                }
                return item;
            }
            finally
            {
                _lock.Release();
            }
        }

        private CacheItemPolicy Policy(ICacheKey cacheKey)
        {
            EnsureParentKeyPolicies(cacheKey.ParentCacheKey);

            CacheItemPolicy policy = new CacheItemPolicy();
            if (cacheKey.Expiration != TimeSpan.Zero)
            {
                policy.AbsoluteExpiration = DateTime.Now.Add(cacheKey.Expiration);
            }
            policy.ChangeMonitors.Add(_cache.CreateCacheEntryChangeMonitor(new string[] { cacheKey.ParentCacheKey?.Key ?? cacheKey.Key }));

            return policy;
        }

        private void EnsureParentKeyPolicies(ICacheKey cacheKey)
        {
            if (cacheKey == null)
            {
                return;
            }

            EnsureParentKeyPolicies(cacheKey.ParentCacheKey);

            if (_cache.Contains(cacheKey.Key))
            {
                return;
            }

            CacheItemPolicy keyPolicy = new CacheItemPolicy();
            if (cacheKey.ParentCacheKey != null)
            {
                keyPolicy.ChangeMonitors.Add(_cache.CreateCacheEntryChangeMonitor(new string[] { cacheKey.ParentCacheKey.Key }));
            }
            _cache.Add(cacheKey.Key, _object, keyPolicy);
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
            }
            finally
            {
                foreach (ICacheKey key in dependencies)
                {
                    _cache.Remove(key.Key);
                }
                _lock.Release();
            }
        }

        public async Task UpdateItemAsync(Func<Task> updateFunctionAsync, params ICacheKey[] dependencies)
        {
            await _lock.WaitAsync().Free();
            try
            {
                await updateFunctionAsync().Free();
            }
            finally
            {
                foreach (ICacheKey key in dependencies)
                {
                    _cache.Remove(key.Key);
                }
                _lock.Release();
            }
        }

        public async Task<T> UpdateItemAsync<T>(Func<Task<T>> updateFunctionAsync, params ICacheKey[] dependencies)
        {
            await _lock.WaitAsync().Free();
            try
            {
                T item = await updateFunctionAsync().Free();
                return item;
            }
            finally
            {
                foreach (ICacheKey key in dependencies)
                {
                    _cache.Remove(key.Key);
                }
                _lock.Release();
            }
        }

        public void RemoveItem(ICacheKey cacheKey)
        {
            if (cacheKey == null)
            {
                throw new ArgumentNullException(nameof(cacheKey));
            }

            _cache.Remove(cacheKey.Key);
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

            if (_cache != null)
            {
                _cache.Dispose();
                _cache = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
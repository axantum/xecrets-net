using AxCrypt.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Common
{
    public class CacheKey : ICacheKey
    {
        private static readonly TimeSpan ItemExpiration = TimeSpan.Zero;

        public static CacheKey RootKey { get; } = new CacheKey();

        private string _key;

        private CacheKey()
            : this("RootKey", null)
        {
        }

        public CacheKey(string key)
            : this(key, RootKey)
        {
        }

        public CacheKey(string key, ICacheKey parentCacheKey)
        {
            _key = key;
            ParentCacheKey = parentCacheKey;
        }

        public CacheKey Subkey(string key)
        {
            CacheKey subkey = new CacheKey(key, this);
            return subkey;
        }

        public ICacheKey ParentCacheKey { get; }

        public string Key
        {
            get
            {
                if (ParentCacheKey == null)
                {
                    return _key;
                }
                return ParentCacheKey.Key + "-" + _key;
            }
        }

        public TimeSpan Expiration { get { return ItemExpiration; } }
    }
}
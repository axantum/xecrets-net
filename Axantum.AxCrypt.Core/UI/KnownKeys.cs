using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.Crypto;

namespace Axantum.AxCrypt.Core.UI
{
    public static class KnownKeys
    {
        private static List<AesKey> _keys = new List<AesKey>();

        public static void Add(AesKey key)
        {
            lock (_keys)
            {
                int i = _keys.IndexOf(key);
                if (i == 0)
                {
                    return;
                }
                if (i >= 0)
                {
                    _keys.RemoveAt(i);
                    _keys.Insert(0, key);
                    return;
                }
                _keys.Add(key);
            }
        }

        public static IEnumerable<AesKey> Keys
        {
            get
            {
                return _keys;
            }
        }

        private static AesKey _defaultEncryptionKey;

        public static AesKey DefaultEncryptionKey
        {
            get
            {
                return _defaultEncryptionKey;
            }
            set
            {
                _defaultEncryptionKey = value;
                Add(value);
            }
        }
    }
}
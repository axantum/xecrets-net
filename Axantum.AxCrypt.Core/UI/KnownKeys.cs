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
            _keys.Add(key);
        }

        public static IEnumerable<AesKey> Keys
        {
            get
            {
                return _keys;
            }
        }
    }
}
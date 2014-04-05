using System;
using System.Collections.Generic;
using System.Linq;

namespace Axantum.AxCrypt.Core.Crypto
{
    public class CryptoFactory
    {
        public static readonly Guid Aes256Id = new Guid("E20F33D4-89E2-4D88-A39C-21DD62FB674F");

        public static readonly Guid Aes128Id = new Guid("2B0CCBB0-B978-4BC3-A293-F97585F06557");

        public static readonly Guid Aes128V1Id = new Guid("1673BBEF-A56A-43AC-AB16-E14D2BAD1CBF");

        public static readonly long DerivationIterations = 1000;

        private Dictionary<Guid, Func<ICryptoFactory>> _factories = new Dictionary<Guid, Func<ICryptoFactory>>();

        public CryptoFactory()
        {
        }

        public void Add(Func<ICryptoFactory> factory)
        {
            _factories.Add(factory().Id, factory);
        }

        public ICryptoFactory Create(Guid id)
        {
            if (id == Guid.Empty)
            {
                return Default;
            }
            Func<ICryptoFactory> factory;
            if (_factories.TryGetValue(id, out factory))
            {
                return factory();
            }
            throw new ArgumentException("CryptoFactory not found.", "id");
        }

        public IEnumerable<Guid> OrderedIds
        {
            get
            {
                return Factory.Instance.Singleton<ICryptoPolicy>().OrderedIds(_factories.Values);
            }
        }

        public ICryptoFactory Default
        {
            get
            {
                return Factory.Instance.Singleton<ICryptoPolicy>().Default(_factories.Values);
            }
        }

        public ICryptoFactory Legacy
        {
            get
            {
                return Factory.Instance.Singleton<ICryptoPolicy>().Legacy(_factories.Values);
            }
        }

        public ICryptoFactory Preferrred
        {
            get
            {
                return _factories.Values.OrderByDescending(f => f().Priority).First()();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axantum.AxCrypt.Core.Crypto
{
    public class CryptoFactory
    {
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
            Func<ICryptoFactory> factory;
            if (_factories.TryGetValue(id, out factory))
            {
                return factory();
            }
            throw new ArgumentException("CryptoFactory not found.", "id");
        }

        public ICryptoFactory Default
        {
            get
            {
                return _factories.Values.OrderByDescending(f => f().Priority).First()();
            }
        }

        public ICryptoFactory Legacy
        {
            get
            {
                return _factories.Values.OrderBy(f => f().Priority).First()();
            }
        }
    }
}
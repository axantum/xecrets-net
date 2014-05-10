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

        public static readonly int DerivationIterations = 1000;

        private Dictionary<Guid, CryptoFactoryCreator> _factories = new Dictionary<Guid, CryptoFactoryCreator>();

        public void Add(CryptoFactoryCreator factory)
        {
            _factories.Add(factory().Id, factory);
        }

        public bool TypeNameExists(string fullName)
        {
            return _factories.Any(c => c.Value().GetType().FullName == fullName);
        }

        public ICryptoFactory Create(Guid id)
        {
            if (id == Guid.Empty)
            {
                return Default;
            }
            CryptoFactoryCreator factory;
            if (_factories.TryGetValue(id, out factory))
            {
                return factory();
            }
            throw new ArgumentException("CryptoFactory not found.", "id");
        }

        /// <summary>
        /// Return a list of CryptoId's in a suitable order of preference and relevance, to be used to
        /// try and match a passphrase against a file.
        /// </summary>
        /// <returns>A list of CryptoId's to try in the order provided.</returns>
        public IEnumerable<Guid> OrderedIds
        {
            get
            {
                Guid defaultId = Default.Id;
                Guid legacyId = Legacy.Id;

                List<Guid> orderedIds = new List<Guid>();
                orderedIds.Add(defaultId);
                orderedIds.AddRange(_factories.Values.Where(f => f().Id != defaultId && f().Id != legacyId).Select(f => f().Id));
                orderedIds.Add(legacyId);

                return orderedIds;
            }
        }

        public ICryptoFactory Default
        {
            get
            {
                return Factory.Instance.Singleton<ICryptoPolicy>().DefaultCryptoFactory(_factories.Values.OrderByDescending(f => f().Priority));
            }
        }

        public ICryptoFactory Legacy
        {
            get
            {
                return Create(Aes128V1Id);
            }
        }

        public ICryptoFactory Minimum
        {
            get
            {
                return Create(Aes128Id);
            }
        }
    }
}
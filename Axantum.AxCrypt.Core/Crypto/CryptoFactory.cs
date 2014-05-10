#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
 *
 * This file is part of AxCrypt.
 *
 * AxCrypt is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * AxCrypt is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AxCrypt.  If not, see <http://www.gnu.org/licenses/>.
 *
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

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

        public ICryptoFactory Create(ICryptoPolicy policy)
        {
            return policy.DefaultCryptoFactory(_factories.Values.OrderByDescending(f => f().Priority));
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
                return Create(Factory.Instance.Singleton<ICryptoPolicy>());
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
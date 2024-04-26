#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
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
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

using System.Reflection;
using AxCrypt.Core.Runtime;

#endregion Coypright and License

using System;
using System.Collections.Generic;
using System.Linq;

using static AxCrypt.Abstractions.TypeResolve;
using System.Diagnostics.CodeAnalysis;

namespace AxCrypt.Core.Crypto
{
    [UnconditionalSuppressMessage("TrimAnalysis", "IL2077", Justification = "Silence the warnings, but be aware that dynamic plugin loading is broken when trimming is enabled.")]
    public class CryptoFactory
    {
        public static readonly int DerivationIterations = 1000;

        private readonly Dictionary<Guid, CryptoFactoryCreator> _factories = new Dictionary<Guid, CryptoFactoryCreator>();

        public CryptoFactory()
        {
        }

        [UnconditionalSuppressMessage("TrimAnalysis", "IL2072", Justification = "Silence the warnings, but be aware that dynamic plugin loading is broken when trimming is enabled.")]
        public CryptoFactory(IEnumerable<Assembly> extraAssemblies)
        {
            Add(() => new V1Aes128CryptoFactory());
            Add(() => new V2Aes128CryptoFactory());
            Add(() => new V2Aes256CryptoFactory());

            IEnumerable<Type> types = TypeDiscovery.Interface(typeof(ICryptoFactory), extraAssemblies);
            foreach (Type type in types)
            {
                Add(() => (ICryptoFactory)(Activator.CreateInstance(type) ?? throw new InvalidOperationException("Internal Program Error, CreateInstance() returned null.")));
            }
        }

        public void Add(CryptoFactoryCreator factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            lock (_factories)
            {
                Guid id = factory().CryptoId;
                _factories[id] = factory;
            }
        }

        public bool TypeNameExists(string fullName)
        {
            lock (_factories)
            {
                return _factories.Any(c => c.Value().GetType().FullName == fullName);
            }
        }

        public ICryptoFactory Create(Guid id)
        {
            if (id == Guid.Empty)
            {
                return New<ICryptoPolicy>().DefaultCryptoFactory(_factories.Values);
            }
            lock (_factories)
            {
                if (_factories.TryGetValue(id, out var factory))
                {
                    return factory();
                }
            }
            throw new ArgumentException("CryptoFactory not found.", nameof(id));
        }

        public ICryptoFactory Create(ICryptoPolicy policy)
        {
            if (policy == null)
            {
                throw new ArgumentNullException(nameof(policy));
            }

            lock (_factories)
            {
                return policy.DefaultCryptoFactory(_factories.Values.OrderByDescending(f => f().Priority));
            }
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
                Guid defaultId = Preferred.CryptoId;
                Guid legacyId = Legacy.CryptoId;

                var orderedIds = new List<Guid>
                {
                    defaultId
                };
                lock (_factories)
                {
                    orderedIds.AddRange(_factories.Values.Where(f => f().CryptoId != defaultId && f().CryptoId != legacyId).Select(f => f().CryptoId));
                }
                orderedIds.Add(legacyId);

                return orderedIds;
            }
        }

        public ICryptoFactory Default(ICryptoPolicy policy)
        {
            if (policy == null)
            {
                throw new ArgumentNullException(nameof(policy));
            }

            lock (_factories)
            {
                return policy.DefaultCryptoFactory(_factories.Values.OrderByDescending(f => f().Priority));
            }
        }

        public ICryptoFactory Preferred
        {
            get
            {
                lock (_factories)
                {
                    return New<ISystemCryptoPolicy>().PreferredCryptoFactory(_factories.Values.OrderByDescending(f => f().Priority));
                }
            }
        }

        public ICryptoFactory Legacy
        {
            get
            {
                return Create(new V1Aes128CryptoFactory().CryptoId);
            }
        }

        public ICryptoFactory Minimum
        {
            get
            {
                return Create(new V2Aes128CryptoFactory().CryptoId);
            }
        }
    }
}

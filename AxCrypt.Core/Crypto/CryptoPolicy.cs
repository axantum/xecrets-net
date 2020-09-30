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

#endregion Coypright and License

using AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AxCrypt.Core.Crypto
{
    public class CryptoPolicy
    {
        private Dictionary<string, ICryptoPolicy> _policies = new Dictionary<string, ICryptoPolicy>();

        public CryptoPolicy(IEnumerable<Assembly> extraAssemblies)
        {
            IEnumerable<Type> policyTypes = TypeDiscovery.Interface(typeof(ICryptoPolicy), extraAssemblies);
            foreach (Type policyType in policyTypes)
            {
                ICryptoPolicy instance = Activator.CreateInstance(policyType) as ICryptoPolicy;
                _policies.Add(instance.Name, instance);
            }
        }

        public ICryptoPolicy Create(string name)
        {
            return _policies[name];
        }

        public IEnumerable<string> PolicyNames
        {
            get
            {
                return _policies.Values.OrderByDescending(p => Resolve.CryptoFactory.Create(p).Priority).Select(p => p.Name);
            }
        }

        public ICryptoPolicy CreateDefault()
        {
            return _policies.Values.OrderByDescending(p => p.Priority).First();
        }
    }
}
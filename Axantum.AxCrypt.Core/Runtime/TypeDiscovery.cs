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
using System.Reflection;

namespace Axantum.AxCrypt.Core.Runtime
{
    public static class TypeDiscovery
    {
        public static IEnumerable<Type> Interface(Type interfaceToDiscover, IEnumerable<Assembly> extraAssemblies)
        {
            List<Type> interfaces = new List<Type>();
            foreach (Assembly assembly in new Assembly[] { interfaceToDiscover.Assembly }.Concat(extraAssemblies))
            {
                try
                {
                    ScanAssemblyForNewInterfaces(interfaceToDiscover, assembly, interfaces);
                }
                catch (TypeLoadException)
                {
                }
            }
            return interfaces;
        }

        private static void ScanAssemblyForNewInterfaces(Type interfaceToDiscover, Assembly assembly, IList<Type> interfaces)
        {
            IEnumerable<Type> types = from t in assembly.GetExportedTypes() where t.GetInterfaces().Contains(interfaceToDiscover) select t;

            foreach (Type t in types)
            {
                if (!interfaces.Any(i => i.FullName == t.FullName))
                {
                    interfaces.Add(t);
                }
            }
        }
    }
}
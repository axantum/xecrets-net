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

using AxCrypt.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Runtime
{
    /// <summary>
    /// Scan external assemblies for implementations of an interface.
    /// </summary>
    /// <remarks>
    /// If there are warnings from the trimming, but no reference to the error code, run the publish from the command line with:
    /// dotnet publish /p:PublishProfile=Properties\PublishProfiles\FolderProfile.pubxml /p:Configuration=Release
    /// </remarks>
    [UnconditionalSuppressMessage("TrimAnalysis", "IL2070", Justification = "Silence the warnings, but be aware that dynamic plugin loading is broken when trimming is enabled.")]
    public static class TypeDiscovery
    {
        public static IEnumerable<Type> Interface(Type interfaceToDiscover, IEnumerable<Assembly> extraAssemblies)
        {
            if (interfaceToDiscover == null)
            {
                throw new ArgumentNullException(nameof(interfaceToDiscover));
            }

            List<Type> interfaces = new List<Type>();
            foreach (Assembly assembly in extraAssemblies)
            {
                try
                {
                    ScanExternalAssemblyForNewInterfaces(interfaceToDiscover, assembly, interfaces);
                }
                catch (TypeLoadException tlex)
                {
                    New<IReport>().Exception(tlex);
                }
            }
            return interfaces;
        }

        [UnconditionalSuppressMessage("TrimAnalysis", "IL2026:RequiresUnreferencedCode", Justification = "Silence the warnings, but be aware that dynamic plugin loading is broken when trimming is enabled.")]
        private static void ScanExternalAssemblyForNewInterfaces(Type interfaceToDiscover, Assembly assembly, IList<Type> interfaces)
        {
            IEnumerable<Type> types = from t in assembly.ExportedTypes where t.GetTypeInfo().ImplementedInterfaces.Contains(interfaceToDiscover) select t;

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

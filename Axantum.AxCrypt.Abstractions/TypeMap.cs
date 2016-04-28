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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using System;
using System.Collections.Generic;
using System.Linq;

namespace Axantum.AxCrypt.Abstractions
{
    /// <summary>
    /// Map a type to a class factory creating instances of that type. This is used as a simple dependency injection vehicle
    /// for types that this library depends on external implementations of for flexibility or unit testing purposes.
    /// </summary>
    public class TypeMap
    {
        /// <summary>
        /// The instance. There can be only one.
        /// </summary>
        private static TypeMap _instance = new TypeMap();

        /// <summary>
        /// The type map
        /// </summary>
        private Dictionary<Type, object> _mapping = new Dictionary<Type, object>();

        private TypeMap()
        {
        }

        public static TypeRegister Register
        {
            get
            {
                return new TypeRegister(_instance._mapping);
            }
        }

        public static TypeResolve Create
        {
            get
            {
                return new TypeResolve(_instance._mapping);
            }
        }
    }
}
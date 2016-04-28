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
using System.Text;

namespace Axantum.AxCrypt.Abstractions
{
    public class TypeResolve
    {
        private IDictionary<Type, object> _mapping;

        public TypeResolve(IDictionary<Type, object> mapping)
        {
            _mapping = mapping;
        }

        /// <summary>
        /// Resolve an instance of the given type. If the type was registered as a singleton, the method delegate registered to provide the instance is
        /// only called once, on the first call.
        /// </summary>
        /// <typeparam name="TResult">The type to resolve.</typeparam>
        /// <returns>An instance of the given type.</returns>
        public static TResult New<TResult>() where TResult : class
        {
            return TypeMap.Create.NewInternal<TResult>();
        }

        /// <summary>
        /// Create an instance of a registered type.
        /// </summary>
        /// <typeparam name="TResult">The type to create an instance of.</typeparam>
        /// <param name="argument">The string argument to the constructor.</param>
        /// <returns>
        /// An instance of the type, according to the rules of the factory. It may be a singleton.
        /// </returns>
        public static TResult New<TResult>(string argument)
        {
            return TypeMap.Create.NewInternal<TResult>(argument);
        }

        /// <summary>
        /// Create an instance of a registered type with an argument to the constructor.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument to the constructor.</typeparam>
        /// <typeparam name="TResult">The type to create an instance of.</typeparam>
        /// <param name="argument">The argument.</param>
        /// <returns>An instance of the type, according to the rules of the factory. It may be a singleton.</returns>
        public static TResult New<TArgument, TResult>(TArgument argument)
        {
            return TypeMap.Create.NewInternal<TArgument, TResult>(argument);
        }

        private TResult NewInternal<TResult>() where TResult : class
        {
            object o;
            if (!_mapping.TryGetValue(typeof(TResult), out o))
            {
                throw new ArgumentException("Unregistered type. Initialize with 'TypeMap.Register.[Singleton|New]<{0}>(() => {{ return new {0}(); }});'".Format(typeof(TResult)));
            }

            TResult value = o as TResult;
            if (value != null)
            {
                return value;
            }

            Func<TResult> function = o as Func<TResult>;
            if (function != null)
            {
                return function();
            }

            Creator<TResult> creator = (Creator<TResult>)o;
            value = creator.CreateFunc();
            _mapping[typeof(TResult)] = value;

            creator.PostAction();
            return value;
        }

        private TResult NewInternal<TResult>(string argument)
        {
            return CreateInternal<string, TResult>(argument);
        }

        private TResult NewInternal<TArgument, TResult>(TArgument argument)
        {
            return CreateInternal<TArgument, TResult>(argument);
        }

        private TResult CreateInternal<TArgument, TResult>(TArgument argument)
        {
            object o;
            if (!_mapping.TryGetValue(typeof(TResult), out o))
            {
                throw new ArgumentException("Unregistered type factory. Initialize with 'TypeMap.Register<{0}, {1}>((argument) => {{ return new {0}(argument); }});'".Format(typeof(TArgument), typeof(TResult)));
            }
            Func<TArgument, TResult> function = (Func<TArgument, TResult>)o;
            return function(argument);
        }
    }
}
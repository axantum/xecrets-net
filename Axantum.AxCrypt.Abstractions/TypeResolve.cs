#region Coypright and License

/*
 * AxCrypt - Copyright 2015, Svante Seleborg, All Rights Reserved
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
        /// Resolve a singleton instance of the given type. The method delegate registered to provide the instance is
        /// only called once, on the first call.
        /// </summary>
        /// <typeparam name="TResult">The type of the singleton to resolve.</typeparam>
        /// <returns>A singleton instance of the given type.</returns>
        public TResult Singleton<TResult>() where TResult : class
        {
            object o;
            if (!_mapping.TryGetValue(typeof(TResult), out o))
            {
                throw new ArgumentException("Unregistered singleton. Initialize with 'FactoryRegistry.Singleton<{0}>(() => {{ return new {0}(); }});'".Format(typeof(TResult)));
            }

            TResult value = o as TResult;
            if (value != null)
            {
                return value;
            }

            Creator<TResult> creator = (Creator<TResult>)o;
            value = creator.CreateFunc();
            _mapping[typeof(TResult)] = value;

            creator.PostAction();
            return value;
        }

        /// <summary>
        /// Create an instance of a registered type.
        /// </summary>
        /// <typeparam name="TResult">The type to create an instance of.</typeparam>
        /// <returns>An instance of the type, according to the rules of the factory. It may be a singleton.</returns>
        public TResult New<TResult>()
        {
            return CreateInternal<TResult>();
        }

        /// <summary>
        /// Create an instance of a registered type.
        /// </summary>
        /// <typeparam name="TResult">The type to create an instance of.</typeparam>
        /// <param name="argument">The argument to the constructor.</param>
        /// <returns>
        /// An instance of the type, according to the rules of the factory. It may be a singleton.
        /// </returns>
        public TResult New<TResult>(string argument)
        {
            return CreateInternal<string, TResult>(argument);
        }

        /// <summary>
        /// Create an instance of a registered type with an argument to the constructor.
        /// </summary>
        /// <typeparam name="TArgument">The type of the argument to the constructor.</typeparam>
        /// <typeparam name="TResult">The type to create an instance of.</typeparam>
        /// <param name="argument">The argument.</param>
        /// <returns>An instance of the type, according to the rules of the factory. It may be a singleton.</returns>
        public TResult New<TArgument, TResult>(TArgument argument)
        {
            return CreateInternal<TArgument, TResult>(argument);
        }

        private TResult CreateInternal<TResult>()
        {
            Func<TResult> function = GetTypeFactory<TResult>();
            return function();
        }

        private TResult CreateInternal<TArgument, TResult>(TArgument argument)
        {
            Func<TArgument, TResult> function = GetTypeFactory<TArgument, TResult>();
            return function(argument);
        }

        private Func<TResult> GetTypeFactory<TResult>()
        {
            object function;
            if (!_mapping.TryGetValue(typeof(TResult), out function))
            {
                throw new ArgumentException("Unregistered type factory. Initialize with 'Factory.Instance.Register<{0}>(() => {{ return new {0}(); }});'".Format(typeof(TResult)));
            }
            return (Func<TResult>)function;
        }

        private Func<TArgument, TResult> GetTypeFactory<TArgument, TResult>()
        {
            object function;
            if (!_mapping.TryGetValue(typeof(TResult), out function))
            {
                throw new ArgumentException("Unregistered type factory. Initialize with 'Factory.Instance.Register<{0}, {1}>((argument) => {{ return new {0}(argument); }});'".Format(typeof(TArgument), typeof(TResult)));
            }
            return (Func<TArgument, TResult>)function;
        }
    }
}
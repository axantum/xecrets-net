#region Coypright and License

/*
 * AxCrypt - Copyright 2013, Svante Seleborg, All Rights Reserved
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

using Axantum.AxCrypt.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axantum.AxCrypt.Core
{
    /// <summary>
    /// Map a type to a class factory creating instances of that type. This is used as a simple dependency injection vehicle
    /// for types that this library depends on external implementations of for flexibility or unit testing purposes.
    /// </summary>
    public class FactoryRegistry
    {
        private Dictionary<Type, object> _mapping = new Dictionary<Type, object>();

        private FactoryRegistry()
        {
        }

        private static FactoryRegistry _instance = new FactoryRegistry();

        /// <summary>
        /// Gets the singleton instance of the FactoryRegistry.
        /// </summary>
        /// <value>
        /// The instance. There can be only one.
        /// </value>
        public static FactoryRegistry Instance
        {
            get
            {
                return _instance;
            }
        }

        /// <summary>
        /// Register a method that creates an instance of the given type. A second registration of the same type
        /// overwrites the first.
        /// </summary>
        /// <typeparam name="TResult">The type to register a factory for.</typeparam>
        /// <param name="creator">The delegate that creates an instance.</param>
        public void Register<TResult>(Func<TResult> creator)
        {
            SetAndDisposeIfDisposable(typeof(Func<TResult>), creator);
        }

        public void Register<TArgument, TResult>(Func<TArgument, TResult> creator)
        {
            SetAndDisposeIfDisposable(typeof(Func<TArgument, TResult>), creator);
        }

        public void Singleton<TArgument>(Func<TArgument> creator)
        {
            SetAndDisposeIfDisposable(typeof(TArgument), creator);
        }

        private void SetAndDisposeIfDisposable(Type type, object value)
        {
            object o;
            if (_mapping.TryGetValue(type, out o))
            {
                DisposeIfDisposable(o);
            }
            _mapping[type] = value;
        }

        public TResult Singleton<TResult>() where TResult : class
        {
            object o;
            if (!_mapping.TryGetValue(typeof(TResult), out o))
            {
                throw new ArgumentException("Unregistered singleton. Initialize with 'FactoryRegistry.Singleton<{0}>(() => {{ return new {0}(); }});'".InvariantFormat(typeof(TResult)));
            }

            TResult value = o as TResult;
            if (value != null)
            {
                return value;
            }

            Func<TResult> creator = (Func<TResult>)o;
            value = creator();
            _mapping[typeof(TResult)] = value;
            return value;
        }

        /// <summary>
        /// Create an instance of a registered type.
        /// </summary>
        /// <typeparam name="TResult">The type to create an instance of.</typeparam>
        /// <returns>An instance of the type, according to the rules of the factory. It may be a singleton.</returns>
        public TResult Create<TResult>()
        {
            object function;
            if (!_mapping.TryGetValue(typeof(Func<TResult>), out function))
            {
                throw new ArgumentException("Unregistered type factory. Initialize with 'FactoryRegistry.Register<{0}>(() => {{ return new {0}(); }});'".InvariantFormat(typeof(TResult)));
            }
            return ((Func<TResult>)function)();
        }

        public TResult Create<TArgument, TResult>(TArgument argument)
        {
            object function;
            if (!_mapping.TryGetValue(typeof(Func<TArgument, TResult>), out function))
            {
                throw new ArgumentException("Unregistered type factory. Initialize with 'FactoryRegistry.Register<{0}, {1}>((argument) => {{ return new {0}(argument); }});'".InvariantFormat(typeof(TArgument), typeof(TResult)));
            }
            return ((Func<TArgument, TResult>)function)(argument);
        }

        /// <summary>
        /// Unregister all factories and instances, dispose if required.
        /// </summary>
        public void Clear()
        {
            foreach (object o in _mapping.Values)
            {
                DisposeIfDisposable(o);
            }
            _mapping.Clear();
        }

        private static void DisposeIfDisposable(object o)
        {
            IDisposable disposable = o as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }
    }
}
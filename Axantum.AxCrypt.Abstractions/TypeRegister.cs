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
    public class TypeRegister
    {
        private IDictionary<Type, object> _mapping;

        public TypeRegister(IDictionary<Type, object> mapping)
        {
            _mapping = mapping;
        }

        /// <summary>
        /// Register a method that creates an instance of the given type. A second registration of the same type
        /// overwrites the first.
        /// </summary>
        /// <typeparam name="TResult">The type to register a factory for.</typeparam>
        /// <param name="creator">The delegate that creates an instance.</param>
        public void New<TResult>(Func<TResult> creator)
        {
            SetAndDisposeIfDisposable(typeof(TResult), creator);
        }

        /// <summary>
        /// Register a method that creates an instance of the given type, taking a single argument.
        /// A second registration of the same type overwrites the first.
        /// </summary>
        /// <typeparam name="TResult">The type to register a factory for.</typeparam>
        /// <param name="creator">The Func<TArg, TResult> delegate that creates an instance.</param>
        public void New<TArgument, TResult>(Func<TArgument, TResult> creator)
        {
            SetAndDisposeIfDisposable(typeof(TResult), creator);
        }

        /// <summary>
        /// Register a method that creates a singleton instance of the given type. This is lazy-evaluated at the first
        /// request to resolve the type.
        /// </summary>
        /// <typeparam name="TResult">The type of the singleton instance.</typeparam>
        /// <param name="creator">The method delegate that creates the singleton.</param>
        public void Singleton<TResult>(Func<TResult> creator)
        {
            Singleton(creator, () => { });
        }

        /// <summary>
        /// Register a method that creates a singleton instance of the given type, and an Action delegate to
        /// execute after creating the instance. Use this to ensure correct dependency ordering.
        /// </summary>
        /// <typeparam name="TResult">The type of the singleton instance.</typeparam>
        /// <param name="creator">The method delegate that creates the singleton.</param>
        /// <param name="postAction">The method delegate to execute after creating the singleton.</param>
        public void Singleton<TResult>(Func<TResult> creator, Action postAction)
        {
            SetAndDisposeIfDisposable(typeof(TResult), new Creator<TResult>(creator, postAction));
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
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
            Register<AxCryptFile>(() => new AxCryptFile());
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
        /// <typeparam name="T">The type to register a factory for.</typeparam>
        /// <param name="creator">The delegate that creates an instance.</param>
        public void Register<T>(Func<T> creator)
        {
            _mapping[typeof(T)] = creator;
        }

        /// <summary>
        /// Create an instance of a registered type.
        /// </summary>
        /// <typeparam name="T">The type to create an instance of.</typeparam>
        /// <returns>An instance of the type, according to the rules of the factory. It may be a singleton.</returns>
        public T Create<T>()
        {
            object function;
            if (!_mapping.TryGetValue(typeof(T), out function))
            {
                throw new ArgumentException("Unregistered type factory. Initialize with 'FactoryRegistry.Register<{0}>(() => {{ return ({0}) ....; }});'".InvariantFormat(typeof(T).ToString()));
            }
            return ((Func<T>)function)();
        }

        /// <summary>
        /// Unregister all factories. Primarily used to support unit testing.
        /// </summary>
        public void Clear()
        {
            _mapping.Clear();
        }
    }
}
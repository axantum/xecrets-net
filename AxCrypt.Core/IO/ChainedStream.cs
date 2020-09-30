using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AxCrypt.Core.IO
{
    /// <summary>
    /// A base class for stream chaining with some common utility methods including mandatory disposal of the chained
    /// stream reference held by this instance.
    /// </summary>
    /// <typeparam name="T">The type of the chained stream.</typeparam>
    public abstract class ChainedStream<T> : Stream where T : Stream
    {
        /// <summary>
        /// Creates the specified chained stream.
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="creator">The creator delegate.</param>
        /// <param name="chained">The chained stream. It will be disposed when this instance is disposed.</param>
        /// <returns></returns>
        /// <remarks>This factory method is used instead of a constructor in order to use type inference and offer a cleaner syntax for the comsumer.</remarks>
        protected static TResult Create<TResult>(Func<T, TResult> creator, T chained) where TResult : Stream
        {
            if (creator == null)
            {
                throw new ArgumentNullException("creator");
            }

            T stream = chained;
            try
            {
                TResult created = creator(chained);
                stream = null;
                return created;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                }
            }
        }

        protected ChainedStream(T chained)
        {
            if (chained == null)
            {
                throw new ArgumentNullException("chained");
            }
            Chained = chained;
        }

        /// <summary>
        /// Gets a reference to the chained stream instance held by this. It will be disposed
        /// when this instance is disposed.
        /// </summary>
        /// <value>
        /// The chained stream.
        /// </value>
        public T Chained { get; private set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeInternal();
            }
            base.Dispose(disposing);
        }

        private void DisposeInternal()
        {
            if (Chained != null)
            {
                Chained.Dispose();
                Chained = null;
            }
        }
    }
}
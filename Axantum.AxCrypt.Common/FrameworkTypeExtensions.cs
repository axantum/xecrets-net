using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Common
{
    public static class FrameworkTypeExtensions
    {
        /// <summary>
        /// Shorthand extension identical to ConfigureAwait(false), i.e. do not require to resume on captured context.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>A configured awaitable task.</returns>
        public static ConfiguredTaskAwaitable Free(this Task task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            return task.ConfigureAwait(false);
        }

        /// <summary>
        /// Shorthand extension identical to ConfigureAwait(false), i.e. do not require to resume on captured context.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task">The task.</param>
        /// <returns>A configured awaitable task.</returns>
        public static ConfiguredTaskAwaitable<T> Free<T>(this Task<T> task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            return task.ConfigureAwait(false);
        }
    }
}
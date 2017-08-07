using Axantum.AxCrypt.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Common
{
    /// <summary>
    /// https://stackoverflow.com/questions/40324300/calling-async-methods-from-non-async-code (Stephen Cleary)
    /// </summary>
    public class TaskRunner
    {
        private Func<Task> _task;

        private TaskRunner(Func<Task> task)
        {
            _task = task;
        }

        /// <summary>
        /// Waits for a Task-returning function, by running it on a thread pool thread. It MUST NOT invoke code on
        /// the UI-thread.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <remarks>
        /// This is DANGEROUS and should be avoided. The Function MUST be able to run on a thread pool thread,
        /// and MUST NOT invoke code on the UI-thread (strictly speaking the caller thread) since that may
        /// cause a deadlock. Only use this as a last resort when under restrictions by external code that cannot
        /// be updated to async. Never use this if the code can be changed to async, or an async API added, no
        /// matter how much work it is.
        /// </remarks>
        public static void WaitFor(Func<Task> task)
        {
            try
            {
                New<IUIThread>().Blocked = true;
                new TaskRunner(task).Wait();
            }
            finally
            {
                New<IUIThread>().Blocked = false;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static T WaitFor<T>(Func<Task<T>> task)
        {
            try
            {
                New<IUIThread>().Blocked = true;
                return new TaskRunner(task).Wait<T>();
            }
            finally
            {
                New<IUIThread>().Blocked = false;
            }
        }

        private void Wait()
        {
            Task.Run(_task).GetAwaiter().GetResult();
        }

        private T Wait<T>()
        {
            return Task.Run((Func<Task<T>>)_task).GetAwaiter().GetResult();
        }
    }
}
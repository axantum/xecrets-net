using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// Waits for a Task-returning function, by running it on a thread pool thread.
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
            new TaskRunner(task).Wait();
        }

        private void Wait()
        {
            Task.Run(_task).GetAwaiter().GetResult();
        }
    }
}
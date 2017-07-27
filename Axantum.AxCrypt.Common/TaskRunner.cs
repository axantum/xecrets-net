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
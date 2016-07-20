using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.Runtime
{
    public class AsyncCompletionEventArgs : EventArgs
    {
        private TaskCompletionSource<object> _completion = new TaskCompletionSource<object>();

        public void Complete()
        {
            _completion.SetResult(default(object));
        }

        public Task Task
        {
            get
            {
                return _completion.Task;
            }
        }
    }
}
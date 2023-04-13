using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Core.Runtime
{
    public class AsyncCompletionEventArgs : EventArgs
    {
        private readonly TaskCompletionSource<object?> _completion = new TaskCompletionSource<object?>();

        public void Complete()
        {
            _completion.SetResult(default);
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

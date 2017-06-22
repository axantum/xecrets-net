using System;
using System.Threading;
using System.Threading.Tasks;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.UI
{
    public class ProgressBackground
    {
        private long _workerCount = 0;

        public event EventHandler<ProgressBackgroundEventArgs> OperationStarted;

        protected virtual void OnOperationStarted(ProgressBackgroundEventArgs e)
        {
            OperationStarted?.Invoke(this, e);
        }

        public event EventHandler<ProgressBackgroundEventArgs> OperationCompleted;

        protected virtual void OnOperationCompleted(ProgressBackgroundEventArgs e)
        {
            OperationCompleted?.Invoke(this, e);
        }

        /// <summary>
        /// Perform a background operation with support for progress bars and cancel.
        /// </summary>
        /// <param name="workFunction">A 'work' delegate, taking a ProgressContext and return a FileOperationStatus. Executed on a background thread. Not the calling thread.</param>
        /// <param name="complete">A 'complete' delegate, taking the final status. Executed on the GUI thread.</param>
        public Task WorkAsync(string name, Func<IProgressContext, Task<FileOperationContext>> workFunction, Action<FileOperationContext> complete, IProgressContext progress)
        {
            return New<IUIThread>().SendToAsync(() => BackgroundWorkWithProgressOnUIThreadAsync(name, workFunction, complete, progress));
        }

        private async Task BackgroundWorkWithProgressOnUIThreadAsync(string name, Func<IProgressContext, Task<FileOperationContext>> work, Action<FileOperationContext> complete, IProgressContext progress)
        {
            ProgressBackgroundEventArgs e = new ProgressBackgroundEventArgs(progress);
            try
            {
                Interlocked.Increment(ref _workerCount);
                OnOperationStarted(e);

                FileOperationContext result = await Task.Run(() => work(progress));
                complete(result);
            }
            finally
            {
                OnOperationCompleted(e);
                Interlocked.Decrement(ref _workerCount);
            }
        }

        /// <summary>
        /// Wait for all operations to complete.
        /// </summary>
        public void WaitForIdle()
        {
            while (Busy)
            {
                New<IUIThread>().Yield();
            }
        }

        public bool Busy
        {
            get
            {
                return _workerCount > 0;
            }
        }
    }
}
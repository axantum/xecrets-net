using AxCrypt.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.UI
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Task WorkAsync(string name, Func<IProgressContext, Task<FileOperationContext>> workFunction, Func<FileOperationContext, Task> complete, IProgressContext progress)
        {
            Task task = New<IUIThread>().SendToAsync(() => BackgroundWorkWithProgressOnUIThreadAsync(name, workFunction, complete, progress));
            return task;
        }

        private async Task BackgroundWorkWithProgressOnUIThreadAsync(string name, Func<IProgressContext, Task<FileOperationContext>> workAsync, Func<FileOperationContext, Task> completeAsync, IProgressContext progress)
        {
            ProgressBackgroundEventArgs e = new ProgressBackgroundEventArgs(progress);
            try
            {
                Interlocked.Increment(ref _workerCount);
                OnOperationStarted(e);

                FileOperationContext result = await Task.Run(() => workAsync(progress));
                await completeAsync(result);
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
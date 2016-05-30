#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
 *
 * This file is part of AxCrypt.
 *
 * AxCrypt is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * AxCrypt is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AxCrypt.  If not, see <http://www.gnu.org/licenses/>.
 *
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Portable;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.Runtime
{
    /// <summary>
    /// Manage a group of worker threads with a maximum level of concurrency.
    /// </summary>
    public class WorkerGroup : IDisposable
    {
        private class ThreadWorkerWrapper : IThreadWorker
        {
            private IThreadWorker _worker;

            public ThreadWorkerWrapper(IThreadWorker worker)
            {
                _worker = worker;
            }

            #region IThreadWorker Members

            /// <summary>
            /// Start the asynchronous execution of the work.
            /// </summary>
            public void Run()
            {
                _worker.Run();
            }

            /// <summary>
            /// Perform blocking wait until this thread has completed execution.
            /// </summary>
            public void Join()
            {
                throw new InvalidOperationException("This instance is managed by a worker group, and join cannot be called explicitly.");
            }

            /// <summary>
            /// Abort this thread - can only be called *before* Run() has been called.
            /// </summary>
            public void Abort()
            {
                _worker.Abort();
            }

            /// <summary>
            /// Returns true if the thread has completed execution.
            /// </summary>
            public bool HasCompleted
            {
                get { throw new InvalidOperationException("This instance is managed by a worker group, and completed status cannot be gotten explicitly."); }
            }

            /// <summary>
            /// Raised just before asynchronous execution starts. Runs on the
            /// original thread, typically the GUI thread.
            /// </summary>
            public event EventHandler<ThreadWorkerEventArgs> Prepare
            {
                add { _worker.Prepare += value; }
                remove { _worker.Prepare -= value; }
            }

            /// <summary>
            /// Raised when asynchronous execution starts. Runs on a different
            /// thread than the caller thread. Do not interact with the GUI here.
            /// </summary>
            public Func<ThreadWorkerEventArgs, Task> WorkAsync
            {
                get { return _worker.WorkAsync; }
                set { _worker.WorkAsync = value; }
            }

            /// <summary>
            /// Raised when all is done. Runs on the original thread, typically
            /// the GUI thread.
            /// </summary>
            public event EventHandler<ThreadWorkerEventArgs> Completing
            {
                add { _worker.Completing += value; }
                remove { _worker.Completing -= value; }
            }

            public event EventHandler<ThreadWorkerEventArgs> Completed
            {
                add { _worker.Completed += value; }
                remove { _worker.Completed -= value; }
            }

            public void Dispose()
            {
                _worker.Dispose();
            }

            #endregion IThreadWorker Members
        }

        private ISemaphore _concurrencyControlSemaphore;

        private int _maxConcurrencyCount;

        private bool _disposed = false;

        private bool _finished = false;

        private ISingleThread _singleThread;

        private readonly object _finishedLock = new object();

        /// <summary>
        /// Instantiate a worker group with no concurrency and implicit progress reporting. Progress
        /// will be reported on the thread calling the constructor.
        /// </summary>
        public WorkerGroup()
            : this(1)
        {
        }

        /// <summary>
        /// Instantiates a worker group with specified maximum concurrency and implicit progress reporting. Progress
        /// will be reported on the thread calling the constructor.
        /// </summary>
        /// <param name="maxConcurrent">The maximum number of worker threads active at any one time</param>
        public WorkerGroup(int maxConcurrent)
            : this(maxConcurrent, new ProgressContext())
        {
        }

        public WorkerGroup(IProgressContext progress)
            : this(1, progress)
        {
        }

        /// <summary
        /// Instantiates a worker group with specified maximum concurrency and external progress reporting. Progress
        /// will be reported on the thread instantiating the ProgressContext used.
        /// </summary>
        /// <param name="maxConcurrent">The maximum number of worker threads active at any one time</param>
        /// <param name="progress">The ProgressContext that receives progress notifications</param>
        public WorkerGroup(int maxConcurrent, IProgressContext progress)
        {
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }

            _concurrencyControlSemaphore = Resolve.Portable.Semaphore(maxConcurrent, maxConcurrent);
            _maxConcurrencyCount = maxConcurrent;
            _singleThread = Resolve.Portable.SingleThread();
            FirstError = new FileOperationContext(String.Empty, ErrorStatus.Success);
            progress.NotifyLevelStart();
            Progress = new WorkerGroupProgressContext(progress, _singleThread);
        }

        /// <summary>
        /// The ProgressContext that is passed to worker threads for progress reporting and cancellation checks. If it was
        /// instantiated by this instance, then progress events are subscribed to and forwarded to the original instantiating
        /// thread, typically the GUI thread. If the ProgressContext was supplied explicitly by the caller when instantiating this
        /// instance, no progress events are subscribed to by this instance.
        /// </summary>
        public IProgressContext Progress { get; private set; }

        /// <summary>
        /// Call when work for this instance has been scheduled. This call will block until all executing threads have terminated.
        /// It will also notify the ProgressContext that all work has finished.
        /// </summary>
        /// <remarks>
        /// Since this call may block, it should never be called from a GUI thread.
        /// </remarks>
        public void WaitAllAndFinish()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("WorkerGroup");
            }
            lock (_finishedLock)
            {
                if (_finished)
                {
                    throw new InvalidOperationException("A worker group can only be finished once, but was called twice.");
                }
                NotifyFinishedInternal();
            }
        }

        private void NotifyFinishedInternal()
        {
            lock (_finishedLock)
            {
                if (_finished)
                {
                    return;
                }

                for (int i = 0; i < _maxConcurrencyCount; ++i)
                {
                    AcquireOneConcurrencyRight();
                }
                _finished = true;
            }
            Progress.NotifyLevelFinished();
        }

        public IThreadWorker CreateWorker(string name)
        {
            return CreateWorker(name, false);
        }

        /// <summary>
        /// Create a ThreadWorker for background work. Concurrency limitations are effective, so this call may block.
        /// </summary>
        /// <remarks>
        /// Since this call may block, it should not be called from the GUI thread if there is a risk of blocking.
        /// </remarks>
        /// <returns></returns>
        public IThreadWorker CreateWorker(string name, bool startSerializedOnUIThread)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("WorkerGroup");
            }
            AcquireOneConcurrencyRight();
            IThreadWorker threadWorker = Resolve.Portable.ThreadWorker(name, Progress, startSerializedOnUIThread);
            threadWorker.Completed += new EventHandler<ThreadWorkerEventArgs>(HandleThreadWorkerCompletedEvent);
            return new ThreadWorkerWrapper(threadWorker);
        }

        /// <summary>
        /// Handles the thread worker completed event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ThreadWorkerEventArgs"/> instance containing the event data.</param>
        private void HandleThreadWorkerCompletedEvent(object sender, ThreadWorkerEventArgs e)
        {
            if (e.Result.ErrorStatus != ErrorStatus.Success)
            {
                lock (_firstErrorLock)
                {
                    if (FirstError.ErrorStatus == ErrorStatus.Success)
                    {
                        FirstError = e.Result;
                    }
                }
            }
            IDisposable disposable = sender as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
            ReleaseOneConcurrencyRight();
        }

        private static readonly object _firstErrorLock = new object();

        /// <summary>
        /// The first error reported. Subsequent errors may be missed.
        /// </summary>
        public FileOperationContext FirstError { get; private set; }

        private void AcquireOneConcurrencyRight()
        {
            _concurrencyControlSemaphore.WaitOne();
        }

        private void ReleaseOneConcurrencyRight()
        {
            _concurrencyControlSemaphore.Release();
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Members

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                DisposeInternal();
            }
            _disposed = true;
        }

        private void DisposeInternal()
        {
            NotifyFinishedInternal();
            if (_concurrencyControlSemaphore != null)
            {
                _concurrencyControlSemaphore.Dispose();
                _concurrencyControlSemaphore = null;
            }
            if (_singleThread != null)
            {
                _singleThread.Dispose();
                _singleThread = null;
            }
        }
    }
}
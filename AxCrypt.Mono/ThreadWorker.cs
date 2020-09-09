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
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Abstractions;
using AxCrypt.Core.Runtime;
using AxCrypt.Core.UI;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Mono
{
    /// <summary>
    /// Perform work on a separate thread.
    /// </summary>
    public class ThreadWorker : IThreadWorker
    {
        private readonly string _name = string.Empty;

        private ManualResetEvent _joined = new ManualResetEvent(false);

        private ThreadWorkerEventArgs _e;

        private bool _startOnUIThread;

        public ThreadWorker(IProgressContext progress)
            : this(progress, false)
        {
        }

        /// <summary>
        /// Create a thread worker.
        /// </summary>
        public ThreadWorker(IProgressContext progress, bool startOnUIThread)
        {
            _startOnUIThread = startOnUIThread;

            _e = new ThreadWorkerEventArgs(new ThreadWorkerProgressContext(progress));
        }

        public ThreadWorker(string name, IProgressContext progress, bool startOnUIThread) : this(progress, startOnUIThread)
        {
            _name = name;
        }

        /// <summary>
        /// Start the asynchronous execution of the work. The instance will dispose of itself once it has
        /// completed, thus allowing a fire-and-forget model.
        /// </summary>
        public void Run()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("ThreadWorker");
            }
            if (_startOnUIThread)
            {
                _e.Progress.EnterSingleThread();
            }
            OnPrepare(_e);

            SynchronizationContext current = SynchronizationContext.Current;
            if (current == null)
            {
                current = new SynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(current);
            }
            Task.Run(async () =>
            {
                await DoWorkAsync(_e);
                current.Send((state) => DoWorkerCompleted(_e), null);
                Dispose();
            }
            );
        }

        /// <summary>
        /// Perform blocking wait until this thread has completed execution. May be called even on a disposed object,
        /// in which case it will return immediately.
        /// </summary>
        public void Join()
        {
            while (!HasCompleted)
            {
            }
        }

        /// <summary>
        /// Abort this thread - can only be called *before* Run() has been called.
        /// </summary>
        public void Abort()
        {
            _e.Result = new FileOperationContext(String.Empty, ErrorStatus.Aborted);
            OnCompleting(_e);
            CompleteWorker(_e);
        }

        /// <summary>
        /// Returns true if the thread has completed execution. May be called even on a disposed
        /// object.
        /// </summary>
        public bool HasCompleted
        {
            get
            {
                lock (_disposeLock)
                {
                    if (_joined == null)
                    {
                        return true;
                    }
                    return _joined.WaitOne(0, false);
                }
            }
        }

        private async Task DoWorkAsync(ThreadWorkerEventArgs e)
        {
            try
            {
                await OnWorkAsync(e);
            }
            catch (OperationCanceledException)
            {
                e.Result = new FileOperationContext(string.Empty, ErrorStatus.Canceled);
            }
            catch (AxCryptException ace)
            {
                New<IReport>().Exception(ace);
                e.Result = new FileOperationContext(ace.DisplayContext, ace.InnerException?.Message, ace.ErrorStatus);
            }
            catch (Exception ex)
            {
                New<IReport>().Exception(ex);
                e.Result = new FileOperationContext(string.Empty, ex.Message, ErrorStatus.Exception);
            }
        }

        private void DoWorkerCompleted(ThreadWorkerEventArgs e)
        {
            try
            {
                OnCompleting(e);
            }
            finally
            {
                CompleteWorker(e);
            }
        }

        /// <summary>
        /// Raised just before asynchronous execution starts. Runs on the
        /// original thread, typically the GUI thread.
        /// </summary>
        public event EventHandler<ThreadWorkerEventArgs> Prepare;

        protected virtual void OnPrepare(ThreadWorkerEventArgs e)
        {
            EventHandler<ThreadWorkerEventArgs> handler = Prepare;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raised when asynchronous execution starts. Runs on a different
        /// thread than the caller thread. Do not interact with the GUI here.
        /// </summary>
        public Func<ThreadWorkerEventArgs, Task> WorkAsync { get; set; }

        protected virtual async Task OnWorkAsync(ThreadWorkerEventArgs e)
        {
            Func<ThreadWorkerEventArgs, Task> commandAsync = WorkAsync;
            if (commandAsync != null)
            {
                await commandAsync.Invoke(e);
            }
        }

        /// <summary>
        /// Raised when all is done. Runs on the original thread, typically
        /// the GUI thread.
        /// </summary>
        public event EventHandler<ThreadWorkerEventArgs> Completing;

        protected virtual void OnCompleting(ThreadWorkerEventArgs e)
        {
            EventHandler<ThreadWorkerEventArgs> handler = Completing;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raised when the underlying worker thread has ended. There is no guarantee on what
        /// thread this will run.
        /// </summary>
        public event EventHandler<ThreadWorkerEventArgs> Completed;

        protected virtual void OnCompleted(ThreadWorkerEventArgs e)
        {
            EventHandler<ThreadWorkerEventArgs> handler = Completed;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Members

        private bool _disposed = false;

        private readonly object _disposeLock = new object();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }
            lock (_disposeLock)
            {
                DisposeInternal();
            }
        }

        private void DisposeInternal()
        {
            if (_disposed)
            {
                return;
            }
            if (_joined != null)
            {
                _joined.Set();
                _joined.Close();
                _joined = null;
            }
            _e.Progress.LeaveSingleThread();
            _disposed = true;
        }

        private void CompleteWorker(ThreadWorkerEventArgs e)
        {
            OnCompleted(e);
            Dispose(true);
        }

        public override string ToString()
        {
            return _name;
        }
    }
}
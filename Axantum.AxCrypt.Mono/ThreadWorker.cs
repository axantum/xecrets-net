#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
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
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.UI;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace Axantum.AxCrypt.Mono
{
    /// <summary>
    /// Perform work on a separate thread.
    /// </summary>
    public class ThreadWorker : IThreadWorker
    {
        private ManualResetEvent _joined = new ManualResetEvent(false);

        private BackgroundWorker _worker;

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
            _worker = new BackgroundWorker();

            _worker.DoWork += new DoWorkEventHandler(_worker_DoWork);
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_worker_RunWorkerCompleted);

            _e = new ThreadWorkerEventArgs(new ThreadWorkerProgressContext(progress));
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
            _worker.RunWorkerAsync();
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
            CompleteWorker();
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

        private void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                OnWork(_e);
                e.Result = _e.Result;
            }
            catch (OperationCanceledException)
            {
                e.Result = new FileOperationContext(String.Empty, ErrorStatus.Canceled);
            }
            catch (AxCryptException ace)
            {
                e.Result = new FileOperationContext(ace.DisplayContext, ErrorStatus.Exception);
                throw;
            }
        }

        private void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    AxCryptException ace = e.Error as AxCryptException;
                    string displayContext = String.Empty;
                    if (ace != null)
                    {
                        displayContext = ace.DisplayContext;
                    }
                    _e.Result = new FileOperationContext(displayContext, ErrorStatus.Exception);
                }
                else
                {
                    _e.Result = (FileOperationContext)e.Result;
                }
                OnCompleting(_e);
            }
            finally
            {
                CompleteWorker();
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
        public event EventHandler<ThreadWorkerEventArgs> Work;

        protected virtual void OnWork(ThreadWorkerEventArgs e)
        {
            EventHandler<ThreadWorkerEventArgs> handler = Work;
            if (handler != null)
            {
                handler(this, e);
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
            if (_worker != null)
            {
                DisposeWorker();
                _worker = null;
            }
            if (_joined != null)
            {
                _joined.Set();
                _joined.Close();
                _joined = null;
            }
            _disposed = true;
        }

        private void CompleteWorker()
        {
            OnCompleted(_e);
            Dispose(true);
        }

        private void DisposeWorker()
        {
            _worker.DoWork -= _worker_DoWork;
            _worker.RunWorkerCompleted -= _worker_RunWorkerCompleted;
            _e.Progress.LeaveSingleThread();

            IDisposable workerAsDisposibleWhichIsPlatformDependent = _worker as IDisposable;
            if (workerAsDisposibleWhichIsPlatformDependent != null)
            {
                workerAsDisposibleWhichIsPlatformDependent.Dispose();
            }
        }
    }
}
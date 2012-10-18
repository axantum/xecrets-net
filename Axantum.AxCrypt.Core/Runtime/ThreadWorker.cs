#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using Axantum.AxCrypt.Core.UI;

namespace Axantum.AxCrypt.Core.Runtime
{
    /// <summary>
    /// Perform work on a separate thread with support for progress and cancellation.
    /// </summary>
    public class ThreadWorker : IDisposable
    {
        private ManualResetEvent _joined = new ManualResetEvent(false);

        private BackgroundWorker _worker;

        private ThreadWorkerEventArgs _e;

        private static readonly object _lock = new object();

        /// <summary>
        /// Create a thread worker.
        /// </summary>
        /// <param name="displayText">A text that may be used in messages as a reference for users.</param>
        /// <param name="work">A 'work' delegate. Executed on a separate thread, not the GUI thread.</param>
        /// <param name="complete">A 'complete' delegate. Executed on the original thread, typically the GUI thread.</param>
        public ThreadWorker(ProgressContext progress)
        {
            _worker = new BackgroundWorker();
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;

            _worker.DoWork += new DoWorkEventHandler(_worker_DoWork);
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_worker_RunWorkerCompleted);

            _e = new ThreadWorkerEventArgs(_worker, progress);
        }

        /// <summary>
        /// Start the asynchronous execution of the work.
        /// </summary>
        public void Run()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("ThreadWorker");
            }
            OnPrepare(_e);
            _worker.RunWorkerAsync();
        }

        public void Join()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("ThreadWorker");
            }
            _joined.WaitOne();
        }

        public bool HasCompleted
        {
            get
            {
                return _joined.WaitOne(TimeSpan.Zero);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Here we really do want to catch everything, and report to the user.")]
        private void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                OnWork(_e);
                e.Result = _e.Result;
            }
            catch (OperationCanceledException)
            {
                e.Result = FileOperationStatus.Canceled;
            }
            catch (Exception ex)
            {
                if (OS.Log.IsWarningEnabled)
                {
                    OS.Log.LogWarning("Exception in background thread '{0}'".InvariantFormat(ex.Message));
                }
                e.Result = FileOperationStatus.Exception;
            }
            return;
        }

        private void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lock (_lock)
            {
                try
                {
                    if (e.Error != null)
                    {
                        _e.Result = FileOperationStatus.UnspecifiedError;
                    }
                    else if (e.Cancelled)
                    {
                        _e.Result = FileOperationStatus.Canceled;
                    }
                    else
                    {
                        _e.Result = (FileOperationStatus)e.Result;
                    }
                    OnCompleted(_e);
                }
                finally
                {
                    DisposeWorker();
                }
            }
            if (OS.Log.IsInfoEnabled)
            {
                OS.Log.LogInfo("BackgroundWorker Disposed.");
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
        /// Raised when progress is reported. Runs on the original thread,
        /// typically the GUI thread.
        /// </summary>
        public event EventHandler<ThreadWorkerEventArgs> Progress;

        protected virtual void OnProgress(ThreadWorkerEventArgs e)
        {
            EventHandler<ThreadWorkerEventArgs> handler = Progress;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Raised when all is done. Runs on the original thread, typically
        /// the GUI thread.
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

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                DisposeWorker();
                if (_joined != null)
                {
                    _joined.Close();
                    _joined = null;
                }
            }

            _disposed = true;
        }

        private void DisposeWorker()
        {
            if (_worker != null)
            {
                _worker.DoWork -= _worker_DoWork;
                _worker.RunWorkerCompleted -= _worker_RunWorkerCompleted;

                IDisposable workerAsDisposibleWhichIsPlatformDependent = _worker as IDisposable;
                if (workerAsDisposibleWhichIsPlatformDependent != null)
                {
                    workerAsDisposibleWhichIsPlatformDependent.Dispose();
                }
                _worker = null;
                _joined.Set();
            }
        }
    }
}
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
using Axantum.AxCrypt.Core.UI;

namespace Axantum.AxCrypt.Core.Runtime
{
    /// <summary>
    /// Perform work on a separate thread with support for progress and cancellation.
    /// </summary>
    public class ThreadWorker : IDisposable
    {
        private BackgroundWorker _worker;

        private ProgressContext _progress;

        private Func<ProgressContext, FileOperationStatus> _work;

        private Action<FileOperationStatus> _complete;

        /// <summary>
        /// Create a thread worker.
        /// </summary>
        /// <param name="displayText">A text that may be used in messages as a reference for users.</param>
        /// <param name="work">A 'work' delegate. Executed on a separate thread, not the GUI thread.</param>
        /// <param name="complete">A 'complete' delegate. Executed on the original thread, typically the GUI thread.</param>
        public ThreadWorker(string displayText, Func<ProgressContext, FileOperationStatus> work, Action<FileOperationStatus> complete)
        {
            _work = work;
            _complete = complete;
            _worker = new BackgroundWorker();
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;
            _worker.DoWork += new DoWorkEventHandler(_worker_DoWork);
            _worker.ProgressChanged += new ProgressChangedEventHandler(_worker_ProgressChanged);
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_worker_RunWorkerCompleted);

            _progress = new ProgressContext(displayText);
            _progress.Progressing += (object sender, ProgressEventArgs e) =>
            {
                if (_worker.CancellationPending)
                {
                    throw new OperationCanceledException();
                }
                _worker.ReportProgress(e.Percent);
            };
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
            OnPrepare(new ThreadWorkerEventArgs(_worker));
            _worker.RunWorkerAsync(_progress);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Here we really do want to catch everything, and report to the user.")]
        private void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("ThreadWorker");
            }
            try
            {
                e.Result = _work((ProgressContext)e.Argument);
            }
            catch (OperationCanceledException)
            {
                e.Result = FileOperationStatus.Canceled;
            }
            catch (Exception ex)
            {
                if (OS.Log.IsWarningEnabled)
                {
                    OS.Log.LogWarning("Exception during encryption '{0}'".InvariantFormat(ex.Message));
                }
                e.Result = FileOperationStatus.Exception;
            }
            return;
        }

        private void _worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("ThreadWorker");
            }
            OnProgress(new ThreadWorkerEventArgs(_worker, e.ProgressPercentage));
        }

        private void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("ThreadWorker");
            }
            try
            {
                _complete((FileOperationStatus)e.Result);
                OnCompleted(new ThreadWorkerEventArgs(_worker));
            }
            finally
            {
                _worker.DoWork -= _worker_DoWork;
                _worker.RunWorkerCompleted -= _worker_RunWorkerCompleted;
                _worker.ProgressChanged -= _worker_ProgressChanged;
                Dispose();
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
                if (_worker != null)
                {
                    IDisposable workerAsDisposibleWhichIsPlatformDependent = _worker as IDisposable;
                    if (workerAsDisposibleWhichIsPlatformDependent != null)
                    {
                        workerAsDisposibleWhichIsPlatformDependent.Dispose();
                    }
                    _worker = null;
                }
            }

            _disposed = true;
        }
    }
}
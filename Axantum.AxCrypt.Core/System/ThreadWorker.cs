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
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.UI;

namespace Axantum.AxCrypt.Core.System
{
    public class ThreadWorker
    {
        private BackgroundWorker _worker;

        private ProgressContext _progress;

        private Func<WorkerArguments, FileOperationStatus> _work;

        private Action<FileOperationStatus> _complete;

        public ThreadWorker(string displayText, Func<WorkerArguments, FileOperationStatus> work, Action<FileOperationStatus> complete)
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

        public void Run()
        {
            ThreadWorkerEventArgs threadWorkerEventArgs = new ThreadWorkerEventArgs(_worker);
            OnPrepare(threadWorkerEventArgs);
            _worker.RunWorkerAsync(new WorkerArguments(_progress));
        }

        private void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                e.Result = _work((WorkerArguments)e.Argument);
            }
            catch (OperationCanceledException)
            {
                e.Result = FileOperationStatus.Canceled;
            }
            catch (Exception ex)
            {
                if (Os.Log.IsWarningEnabled)
                {
                    Os.Log.Warning("Exception during encryption '{0}'".InvariantFormat(ex.Message));
                }
                e.Result = FileOperationStatus.Exception;
            }
            return;
        }

        private void _worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ThreadWorkerEventArgs threadWorkerEventArgs = new ThreadWorkerEventArgs(_worker);
            threadWorkerEventArgs.ProgressPercentage = e.ProgressPercentage;
            OnProgress(threadWorkerEventArgs);
        }

        private void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                _complete((FileOperationStatus)e.Result);
                ThreadWorkerEventArgs threadWorkerEventArgs = new ThreadWorkerEventArgs(_worker);
                threadWorkerEventArgs.Cancelled = e.Cancelled;
                threadWorkerEventArgs.Result = (FileOperationStatus)e.Result;
                OnCompleted(threadWorkerEventArgs);
            }
            finally
            {
                _worker.DoWork -= _worker_DoWork;
                _worker.RunWorkerCompleted -= _worker_RunWorkerCompleted;
                _worker.ProgressChanged -= _worker_ProgressChanged;
                IDisposable workerAsDisposibleWhichIsPlatformDependent = _worker as IDisposable;
                if (workerAsDisposibleWhichIsPlatformDependent != null)
                {
                    workerAsDisposibleWhichIsPlatformDependent.Dispose();
                }
            }
            if (Os.Log.IsInfoEnabled)
            {
                Os.Log.Info("Disposing BackgroundWorker");
            }
        }

        public event EventHandler<ThreadWorkerEventArgs> Prepare;

        protected virtual void OnPrepare(ThreadWorkerEventArgs e)
        {
            EventHandler<ThreadWorkerEventArgs> handler = Prepare;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<ThreadWorkerEventArgs> Progress;

        protected virtual void OnProgress(ThreadWorkerEventArgs e)
        {
            EventHandler<ThreadWorkerEventArgs> handler = Progress;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<ThreadWorkerEventArgs> Completed;

        protected virtual void OnCompleted(ThreadWorkerEventArgs e)
        {
            EventHandler<ThreadWorkerEventArgs> handler = Completed;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
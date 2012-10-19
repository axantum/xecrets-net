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
using System.Linq;
using System.Text;
using System.Threading;
using Axantum.AxCrypt.Core.UI;

namespace Axantum.AxCrypt.Core.Runtime
{
    public class WorkerGroup : IDisposable
    {
        private Semaphore _concurrencyControlSemaphore;

        private int _maxConcurrencyCount;

        private bool _disposed = false;

        public WorkerGroup()
            : this(1)
        {
        }

        public WorkerGroup(ProgressContext progress)
            : this(1, progress)
        {
        }

        public WorkerGroup(int maxConcurrent)
            : this(maxConcurrent, new ProgressContext())
        {
            SynchronizationContext context = SynchronizationContext.Current;
            Progress.Progressing += (object sender, ProgressEventArgs e) =>
                {
                    context.Send((object state) =>
                        {
                            OnProgressing((ProgressEventArgs)state);
                        }, e);
                };
        }

        public WorkerGroup(int maxConcurrent, ProgressContext progress)
        {
            _concurrencyControlSemaphore = new Semaphore(maxConcurrent, maxConcurrent);
            _maxConcurrencyCount = maxConcurrent;
            LastError = FileOperationStatus.Success;
            Progress = progress;
        }

        public ProgressContext Progress { get; private set; }

        public event EventHandler<ProgressEventArgs> Progressing;

        protected virtual void OnProgressing(ProgressEventArgs e)
        {
            EventHandler<ProgressEventArgs> handler = Progressing;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void AcquireOne()
        {
            _concurrencyControlSemaphore.WaitOne();
        }

        public void ReleaseOne()
        {
            _concurrencyControlSemaphore.Release();
        }

        public void AcquireAll()
        {
            for (int i = 0; i < _maxConcurrencyCount; ++i)
            {
                _concurrencyControlSemaphore.WaitOne();
            }
        }

        public void WaitAll()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("WorkerGroup");
            }

            lock (_threadWorkers)
            {
                foreach (ThreadWorker threadWorker in _threadWorkers)
                {
                    threadWorker.Join();
                }
                DisposeCompletedWorkerThreads();
            }
        }

        private void DisposeCompletedWorkerThreads()
        {
            lock (_threadWorkers)
            {
                for (int i = _threadWorkers.Count - 1; i >= 0; --i)
                {
                    if (_threadWorkers[i].HasCompleted)
                    {
                        _threadWorkers[i].Dispose();
                        _threadWorkers.RemoveAt(i);
                    }
                }
            }
        }

        private List<ThreadWorker> _threadWorkers = new List<ThreadWorker>();

        public ThreadWorker CreateWorker()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("WorkerGroup");
            }
            DisposeCompletedWorkerThreads();
            ThreadWorker threadWorker = new ThreadWorker(Progress);
            threadWorker.Completed += new EventHandler<ThreadWorkerEventArgs>(HandleThreadWorkerCompleted);
            lock (_threadWorkers)
            {
                _threadWorkers.Add(threadWorker);
            }
            return threadWorker;
        }

        private void HandleThreadWorkerCompleted(object sender, ThreadWorkerEventArgs e)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("WorkerGroup");
            }
            DisposeCompletedWorkerThreads();
            if (e.Result != FileOperationStatus.Success)
            {
                LastError = e.Result;
            }
        }

        public FileOperationStatus LastError { get; private set; }

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
                WaitAll();
                if (_concurrencyControlSemaphore != null)
                {
                    _concurrencyControlSemaphore.Close();
                    _concurrencyControlSemaphore = null;
                }
            }

            _disposed = true;
        }
    }
}
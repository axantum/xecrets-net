#region Coypright and License

/*
 * AxCrypt - Copyright 2013, Svante Seleborg, All Rights Reserved
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

using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Axantum.AxCrypt.Core.UI
{
    public class ParallelFileOperation : IDisposable
    {
        private IUIThread _uiThread;

        public ParallelFileOperation(IUIThread uiThread)
        {
            _uiThread = uiThread;
        }

        private Semaphore _interactionSemaphore = new Semaphore(1, 1);

        private void SerializedOnUIThread(Action action)
        {
            if (_uiThread.IsOnUIThread)
            {
                action();
                return;
            }
            _interactionSemaphore.WaitOne();
            Action extendedAction = () =>
            {
                try
                {
                    action();
                }
                finally
                {
                    _interactionSemaphore.Release();
                }
            };
            _uiThread.RunOnUIThread(extendedAction);
        }

        /// <summary>
        /// Does an operation on a list of files, with parallelism.
        /// </summary>
        /// <param name="files">The files to operation on.</param>
        /// <param name="work">The work to do for each file.</param>
        /// <param name="allComplete">The completion callback after *all* files have been processed.</param>
        public virtual void DoFiles(IEnumerable<IRuntimeFileInfo> files, Func<IRuntimeFileInfo, IProgressContext, FileOperationStatus> work, Action<FileOperationStatus> allComplete)
        {
            WorkerGroup workerGroup = null;
            Instance.ProgressBackground.Work(
                (IProgressContext progress) =>
                {
                    using (workerGroup = new WorkerGroup(OS.Current.MaxConcurrency, progress))
                    {
                        foreach (IRuntimeFileInfo file in files)
                        {
                            IThreadWorker worker = workerGroup.CreateWorker();
                            IRuntimeFileInfo closureOverCopyOfLoopVariableFile = file;
                            SerializedOnUIThread(() =>
                            {
                                if (workerGroup.FirstError != FileOperationStatus.Success)
                                {
                                    worker.Abort();
                                    return;
                                }
                                worker.Work += (sender, e) =>
                                {
                                    e.Result = work(closureOverCopyOfLoopVariableFile, new CancelContext(progress));
                                };
                                worker.Run();
                            });
                            if (workerGroup.FirstError != FileOperationStatus.Success)
                            {
                                break;
                            }
                        }
                        workerGroup.WaitAllAndFinish();
                        return workerGroup.FirstError;
                    }
                },
                (FileOperationStatus status) =>
                {
                    allComplete(status);
                });
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeInternal();
            }
        }

        private void DisposeInternal()
        {
            if (_interactionSemaphore != null)
            {
                _interactionSemaphore.Close();
                _interactionSemaphore = null;
            }
        }
    }
}
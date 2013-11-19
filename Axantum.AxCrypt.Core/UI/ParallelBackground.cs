using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;

namespace Axantum.AxCrypt.Core.UI
{
    public class ParallelBackground : IDisposable
    {
        public ParallelBackground()
        {
        }

        private Semaphore _interactionSemaphore = new Semaphore(1, 1);

        private void SerializedOnUIThread(Action action)
        {
            if (Instance.UIThread.IsOnUIThread)
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
            Instance.UIThread.RunOnUIThread(extendedAction);
        }

        public void DoFiles(IEnumerable<IRuntimeFileInfo> files, Action<IRuntimeFileInfo, IProgressContext> work, Action<FileOperationStatus> complete)
        {
            DoFiles(files,
                (file, worker, progress) =>
                {
                    worker.Work += (sender, e) =>
                    {
                        work(file, progress);
                    };
                    worker.Run();
                }, complete);
        }

        public void DoFiles(IEnumerable<IRuntimeFileInfo> files, Action<IRuntimeFileInfo, IThreadWorker, IProgressContext> work, Action<FileOperationStatus> complete)
        {
            WorkerGroup workerGroup = null;
            Instance.BackgroundWork.Work(
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
                                work(closureOverCopyOfLoopVariableFile, worker, new CancelContext(progress));
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
                    complete(status);
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
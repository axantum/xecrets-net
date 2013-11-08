using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Axantum.AxCrypt.Core.UI
{
    public class Background : IDisposable
    {
        public Background()
        {
        }

        public void RunOnUIThread(Action action)
        {
            if (!Instance.IUIThread.IsOnUIThread)
            {
                Instance.IUIThread.RunOnUIThread(action);
            }
            else
            {
                action();
            }
        }

        private Semaphore _interactionSemaphore = new Semaphore(1, 1);

        private void SerializedOnUIThread(Action action)
        {
            if (!Instance.IUIThread.IsOnUIThread)
            {
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
                Instance.IUIThread.RunOnUIThread(extendedAction);
            }
            else
            {
                action();
            }
        }

        public void ProcessFiles(IEnumerable<string> files, Action<string, IThreadWorker, ProgressContext> processFile)
        {
            WorkerGroup workerGroup = null;
            Instance.IBackgroundWork.BackgroundWorkWithProgress(
                (ProgressContext progress) =>
                {
                    progress.AddItems(files.Count());
                    using (workerGroup = new WorkerGroup(OS.Current.MaxConcurrency, progress))
                    {
                        foreach (string file in files)
                        {
                            IThreadWorker worker = workerGroup.CreateWorker();
                            string closureOverCopyOfLoopVariableFile = file;
                            SerializedOnUIThread(() =>
                            {
                                processFile(closureOverCopyOfLoopVariableFile, worker, progress);
                            });
                            if (workerGroup.FirstError != FileOperationStatus.Success)
                            {
                                break;
                            }
                            progress.AddItems(-1);
                        }
                        workerGroup.WaitAllAndFinish();
                        return workerGroup.FirstError;
                    }
                },
                (FileOperationStatus status) =>
                {
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
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
    public class Background : IDisposable
    {
        public Background()
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

        public void ProcessFiles(IEnumerable<IRuntimeFileInfo> files, Action<IRuntimeFileInfo, IProgressContext> processFile)
        {
            ProcessFiles(files.Select(f => f.FullName), (file, worker, progress) =>
            {
                worker.Work += (sender, e) =>
                {
                    processFile(OS.Current.FileInfo(file), progress);
                };
                worker.Run();
            });
        }

        public void ProcessFiles(IEnumerable<string> files, Action<string, IThreadWorker, IProgressContext> processFile)
        {
            WorkerGroup workerGroup = null;
            Instance.BackgroundWork.BackgroundWorkWithProgress(
                (IProgressContext progress) =>
                {
                    using (workerGroup = new WorkerGroup(OS.Current.MaxConcurrency, progress))
                    {
                        foreach (string file in files)
                        {
                            IThreadWorker worker = workerGroup.CreateWorker();
                            string closureOverCopyOfLoopVariableFile = file;
                            SerializedOnUIThread(() =>
                            {
                                if (workerGroup.FirstError != FileOperationStatus.Success)
                                {
                                    worker.Abort();
                                    return;
                                }
                                processFile(closureOverCopyOfLoopVariableFile, worker, new CancelContext(progress));
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
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
        private IUIThread _uiThread;

        private IBackgroundWork _backgroundWork;

        private Background(IUIThread uiThread, IBackgroundWork backgroundWork)
        {
            _uiThread = uiThread;
            _backgroundWork = backgroundWork;
        }

        private static Background _background;

        public static Background Instance
        {
            get
            {
                if (_background == null)
                {
                    _background = new Background(FactoryRegistry.Instance.Create<IUIThread>(), FactoryRegistry.Instance.Create<IBackgroundWork>());
                }
                return _background;
            }
            set
            {
                if (_background != null)
                {
                    _background.Dispose();
                }
                _background = value;
            }
        }

        public void RunOnUIThread(Action action)
        {
            if (!_uiThread.IsOnUIThread)
            {
                _uiThread.RunOnUIThread(action);
            }
            else
            {
                action();
            }
        }

        private Semaphore _interactionSemaphore = new Semaphore(1, 1);

        private void SerializedOnUIThread(Action action)
        {
            if (!_uiThread.IsOnUIThread)
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
                _uiThread.RunOnUIThread(extendedAction);
            }
            else
            {
                action();
            }
        }

        public void ProcessFiles(IEnumerable<string> files, Action<string, IThreadWorker, ProgressContext> processFile)
        {
            WorkerGroup workerGroup = null;
            _backgroundWork.BackgroundWorkWithProgress(
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
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

        public Background(IUIThread uiThread)
        {
            _uiThread = uiThread;
        }

        public void ThreadSafeUI(Action action)
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

        public void InteractionSafeUI(Action action)
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Abstractions
{
    /// <summary>
    /// Represents the basic cancelation token. Can be used for implementing pair operation ( Open / Close, ShowPopup / HidePopup, etc.)
    /// </summary>
    public class Releaser : IDisposable
    {
        private bool _isDisposed;

        public Action<Releaser>? OnDisposeAction { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (OnDisposeAction != null)
            {
                OnDisposeAction.Invoke(this);
                OnDisposeAction = null;
            }

            _isDisposed = true;
        }

        ~Releaser()
        {
            Dispose(false);
        }
    }
}
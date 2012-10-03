using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.Session;

namespace Axantum.AxCrypt
{
    internal class PersistentState : Component, ISupportInitialize
    {
        public PersistentState(IContainer container)
        {
            container.Add(this);
        }

        private bool _disposed = false;

        public FileSystemState Current { get; set; }

        #region ISupportInitialize Members

        public void BeginInit()
        {
        }

        public void EndInit()
        {
            if (DesignMode)
            {
                return;
            }
            Current = new FileSystemState();
        }

        #endregion ISupportInitialize Members

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            if (disposing)
            {
                if (Current != null)
                {
                    Current.Dispose();
                    Current = null;
                }
            }
            _disposed = true;
            base.Dispose(disposing);
        }
    }
}
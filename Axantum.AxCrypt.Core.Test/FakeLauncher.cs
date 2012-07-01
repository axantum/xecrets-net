using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.System;

namespace Axantum.AxCrypt.Core.Test
{
    internal class FakeLauncher : ILauncher
    {
        private string _path;

        public FakeLauncher(string path)
        {
            _path = path;
        }

        protected virtual void OnExited(EventArgs e)
        {
            EventHandler handler = Exited;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #region ILauncher Members

        public event EventHandler Exited;

        public bool HasExited
        {
            get { return true; }
        }

        public bool WasStarted
        {
            get { return true; }
        }

        public string Path
        {
            get { return _path; }
        }

        #endregion ILauncher Members

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion IDisposable Members
    }
}
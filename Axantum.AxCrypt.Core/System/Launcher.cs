using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.System
{
    public class Launcher : ILauncher
    {
        private Process _process;

        public Launcher(string path)
        {
            _process = Process.Start(path);
            if (_process == null)
            {
                return;
            }
            _process.Exited += new EventHandler(Process_Exited);
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            OnExited(e);
        }

        #region ILauncher Members

        public event EventHandler Exited;

        public bool HasExited
        {
            get { return _process.HasExited; }
        }

        public bool WasStarted
        {
            get
            {
                return _process != null;
            }
        }

        #endregion ILauncher Members

        protected virtual void OnExited(EventArgs e)
        {
            EventHandler handler = Exited;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }
            if (_process == null)
            {
                return;
            }
            _process.Dispose();
            _process = null;
        }

        public string Path
        {
            get { return _process.StartInfo.FileName; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Members
    }
}
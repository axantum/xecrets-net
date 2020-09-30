#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
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
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Core;
using AxCrypt.Core.Runtime;
using System;
using System.Diagnostics;
using System.Linq;

namespace AxCrypt.Mono
{
    public class Launcher : ILauncher
    {
        private readonly object _disposeLock = new object();

        private Process _process;

        public void Launch(string path)
        {
            _process = Process.Start(path);
            if (_process == null)
            {
                return;
            }
            WasStarted = true;
            Path = _process.StartInfo.FileName;
            Name = _process.ProcessName;
            if (OS.Current.CanTrackProcess)
            {
                // This causes hang-on-exit on at least Mac OS X
                _process.EnableRaisingEvents = true;
                _process.Exited += Process_Exited;
            }
            else
            {
                _process.Dispose();
                _process = null;
            }
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            if (!HasRelatedProcess)
            {
                OnExited(e);
            }
            lock (_disposeLock)
            {
                if (_process != null)
                {
                    _process.Exited -= Process_Exited;
                    _process.WaitForExit();
                }
            }
            DisposeInternal();
        }

        private bool HasRelatedProcess
        {
            get
            {
                int currentSessionId = Process.GetCurrentProcess().SessionId;
                return Process.GetProcesses().Where(p => p.SessionId == currentSessionId).Select(p => p.ProcessName).Contains(Name);
            }
        }

        #region ILauncher Members

        public event EventHandler Exited;

        public bool HasExited
        {
            get
            {
                lock (_disposeLock)
                {
                    return !HasRelatedProcess && (_process == null || _process.HasExited);
                }
            }
        }

        public bool WasStarted
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
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
            DisposeInternal();
        }

        private void DisposeInternal()
        {
            lock (_disposeLock)
            {
                if (_process == null)
                {
                    return;
                }
                _process.Dispose();
                _process = null;
            }
        }

        public string Path
        {
            get; private set;
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
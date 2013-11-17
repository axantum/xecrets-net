#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Permissions;
using System.Text;

namespace Axantum.AxCrypt.Mono
{
    [HostProtection(SecurityAction.LinkDemand, SharedState = true, Synchronization = true, ExternalProcessMgmt = true, SelfAffectingProcessMgmt = true), PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    internal class Launcher : ILauncher
    {
        private Process _process;

        private string _path;

        public Launcher(string path)
        {
            _process = Process.Start(path);
            if (_process == null)
            {
                return;
            }
            WasStarted = true;
            _path = _process.StartInfo.FileName;
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
            OnExited(e);
            _process.Exited -= Process_Exited;
            _process.WaitForExit();
            _process.Dispose();
            _process = null;
        }

        #region ILauncher Members

        public event EventHandler Exited;

        public bool HasExited
        {
            get { return _process == null || _process.HasExited; }
        }

        public bool WasStarted
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
            if (_process == null)
            {
                return;
            }
            _process.Dispose();
            _process = null;
        }

        public string Path
        {
            get { return _path; }
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
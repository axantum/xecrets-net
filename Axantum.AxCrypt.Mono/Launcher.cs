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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.System;

namespace Axantum.AxCrypt.Mono
{
    internal class Launcher : ILauncher
    {
        private Process _process;

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "The code has full trust anyway.")]
        public Launcher(string path)
        {
            _process = Process.Start(path);
            if (_process == null)
            {
                return;
            }
            if (OS.Current.CanTrackProcess)
            {
                // This causes hang-on-exit on at least Mac OS X
                _process.EnableRaisingEvents = true;
                _process.Exited += Process_Exited;
            }
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            OnExited(e);
        }

        #region ILauncher Members

        public event EventHandler Exited;

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "The code has full trust anyway.")]
        public bool HasExited
        {
            get { return !WasStarted || _process.HasExited; }
        }

        public bool WasStarted
        {
            get
            {
                return OS.Current.CanTrackProcess && _process != null;
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

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "The code has full trust anyway.")]
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
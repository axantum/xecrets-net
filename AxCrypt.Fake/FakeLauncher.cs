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

using AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace AxCrypt.Fake
{
    public class FakeLauncher : ILauncher
    {
        [AllowNull]
        private string _path;

        public virtual void Launch(string path)
        {
            _path = path;
            HasExited = false;
            WasStarted = true;
        }

        protected virtual void OnExited(EventArgs e)
        {
            Exited?.Invoke(this, e);
        }

        public void RaiseExited()
        {
            OnExited(new EventArgs());
        }

        #region ILauncher Members

        public event EventHandler? Exited;

        public bool HasExited { get; set; }

        public bool WasStarted { get; set; }

        public string Path
        {
            get { return _path; }
        }

        public string Name { get; set; } = string.Empty;

        #endregion ILauncher Members

        protected virtual void Dispose(bool disposing)
        {
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

#region Coypright and License

/*
 * AxCrypt - Copyright 2013, Svante Seleborg, All Rights Reserved
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

using Axantum.AxCrypt.Core.UI;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    internal class FakeProgressBackground : IProgressBackground
    {
        public void Work(Func<IProgressContext, FileOperationStatus> work, Action<FileOperationStatus> complete)
        {
            Busy = true;
            OnWorkStatusChanged();
            FileOperationStatus status = work(new ProgressContext());
            complete(status);
            Busy = false;
            OnWorkStatusChanged();
        }

        public void WaitForIdle()
        {
        }

        public event EventHandler WorkStatusChanged;

        protected virtual void OnWorkStatusChanged()
        {
            EventHandler handler = WorkStatusChanged;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        public bool Busy
        {
            get;
            set;
        }
    }
}
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

using AxCrypt.Abstractions;
using AxCrypt.Common;
using AxCrypt.Core.Runtime;
using AxCrypt.Core.UI;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AxCrypt.Fake
{
    public class FakeProgressBackground : IProgressBackground
    {
        private string _name;

        public async Task WorkAsync(string name, Func<IProgressContext, Task<FileOperationContext>> workAsync, Func<FileOperationContext, Task> completeAsync, IProgressContext progress)
        {
            _name = name;

            Busy = true;
            OnWorkStatusChanged();
            FileOperationContext status = new FileOperationContext(String.Empty, ErrorStatus.Unknown);
            try
            {
                status = await Task.Run(async () => await workAsync(progress).Free());
            }
            catch (OperationCanceledException)
            {
                status = new FileOperationContext(String.Empty, ErrorStatus.Canceled);
            }
            await completeAsync(status);
            Busy = false;
            OnWorkStatusChanged();
        }

        public void WaitForIdle()
        {
            while (Busy)
            {
                Thread.Sleep(0);
            }
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

        public override string ToString()
        {
            return _name;
        }
    }
}
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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Axantum.AxCrypt
{
    public class UIThread : IUIThread
    {
        private Control _control;

        private SynchronizationContext _context;

        public UIThread(Control control)
        {
            _control = control;
            _context = SynchronizationContext.Current;
        }

        public bool IsOn
        {
            get { return !_control.InvokeRequired; }
        }

        public void SendTo(Action action)
        {
            DoOnUIThreadInternal(action, _context.Send);
        }

        public Task SendToAsync(Func<Task> action)
        {
            return DoOnUIThreadInternal(action, _context.Send, new TaskCompletionSource<object>());
        }

        public async void PostTo(Action action)
        {
            DoOnUIThreadInternal(action, _context.Post);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This is to marshal the exception possibly between threads and then throw a new one.")]
        private async void DoOnUIThreadInternal(Action action, Action<SendOrPostCallback, object> method)
        {
            await DoOnUIThreadInternal(() => { action(); return Constant.CompletedTask; }, method, null);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This is to marshal the exception possibly between threads and then throw a new one.")]
        private async Task DoOnUIThreadInternal(Func<Task> action, Action<SendOrPostCallback, object> method, TaskCompletionSource<object> completion)
        {
            if (IsOn)
            {
                await action();
                return;
            }

            Exception exception = null;
            method(async (state) =>
            {
                try
                {
                    await action();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                completion?.SetResult(null);
            }, null);
            await (completion?.Task ?? Constant.CompletedTask);
            if (exception is AxCryptException)
            {
                throw exception;
            }
            if (exception != null)
            {
                throw new InvalidOperationException("Exception on UI Thread", exception);
            }
        }

        public void Yield()
        {
            Application.DoEvents();
        }

        public void Exit()
        {
            Application.Exit();
        }
    }
}
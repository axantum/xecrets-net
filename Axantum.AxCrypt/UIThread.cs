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
using System.Linq;
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
            return DoOnUIThreadInternal(action, _context.Send);
        }

        public void PostTo(Action action)
        {
            DoOnUIThreadInternal(action, _context.Post);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This is to marshal the exception possibly between threads and then throw a new one.")]
        private void DoOnUIThreadInternal(Action action, Action<SendOrPostCallback, object> method)
        {
            if (IsOn)
            {
                action();
                return;
            }
            Exception exception = null;
            method((state) => { try { action(); } catch (Exception ex) { exception = ex; } }, null);
            if (exception is AxCryptException)
            {
                throw exception;
            }
            if (exception != null)
            {
                throw new InvalidOperationException("Exception on UI Thread", exception);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This is to marshal the exception possibly between threads and then throw a new one.")]
        private Task DoOnUIThreadInternal(Func<Task> action, Action<SendOrPostCallback, object> method)
        {
            if (IsOn)
            {
                return action();
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
            }, null);
            if (exception is AxCryptException)
            {
                throw exception;
            }
            if (exception != null)
            {
                throw new InvalidOperationException("Exception on UI Thread", exception);
            }
            return Task.FromResult<object>(null);
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
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

        public bool IsOnUIThread
        {
            get { return !_control.InvokeRequired; }
        }

        public void RunOnUIThread(Action action)
        {
            DoOnUIThreadInternal(action, _context.Send);
        }

        public void PostOnUIThread(Action action)
        {
            DoOnUIThreadInternal(action, _context.Post);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This is to marshal the exception possibly between threads and then throw a new one.")]
        private void DoOnUIThreadInternal(Action action, Action<SendOrPostCallback, object> method)
        {
            if (IsOnUIThread)
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

        public void Yield()
        {
            Application.DoEvents();
        }
    }
}
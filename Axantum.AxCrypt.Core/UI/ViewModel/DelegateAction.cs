#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    /// <summary>
    /// An IAction whose delegates can be attached for Execute(T) and CanExecute(T).
    /// </summary>
    public class DelegateAction<T> : IAction
    {
        private Action<T> _executeMethod;

        private Func<T, bool> _canExecuteMethod;

        public DelegateAction(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
        {
            if (executeMethod == null)
            {
                throw new ArgumentNullException(nameof(executeMethod));
            }
            if (canExecuteMethod == null)
            {
                throw new ArgumentNullException(nameof(canExecuteMethod));
            }

            _executeMethod = executeMethod;
            _canExecuteMethod = canExecuteMethod;
        }

        public DelegateAction(Action<T> executeMethod)
            : this(executeMethod, (parameter) => true)
        {
        }

        public bool CanExecute(object parameter)
        {
            return _canExecuteMethod(parameter != null ? (T)parameter : default(T));
        }

        public void Execute(object parameter)
        {
            if (!CanExecute(parameter))
            {
                throw new InvalidOperationException("Execute() invoked when it cannot execute.");
            }
            _executeMethod((T)parameter);
        }

        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            EventHandler handler = CanExecuteChanged;
            if (handler != null)
            {
                Resolve.UIThread.RunOnUIThread(() => handler(this, new EventArgs()));
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "Method name taken from DelegateCommand implementation by Microsoft Patterns & Practices.")]
        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged();
        }
    }
}
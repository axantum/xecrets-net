using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public class AsyncDelegateAction<T> : IAsyncAction
    {
        private Func<T, Task> _executeMethodAsync;

        private Func<T, bool> _canExecuteMethod;

        public AsyncDelegateAction(Func<T, Task> executeMethodAsync, Func<T, bool> canExecuteMethod)
        {
            if (executeMethodAsync == null)
            {
                throw new ArgumentNullException(nameof(executeMethodAsync));
            }
            if (canExecuteMethod == null)
            {
                throw new ArgumentNullException(nameof(canExecuteMethod));
            }

            _executeMethodAsync = executeMethodAsync;
            _canExecuteMethod = canExecuteMethod;
        }

        public AsyncDelegateAction(Func<T, Task> executeMethodAsync)
            : this(executeMethodAsync, (parameter) => true)
        {
        }

        public bool CanExecute(object parameter)
        {
            return _canExecuteMethod(parameter != null ? (T)parameter : default(T));
        }

        public async void Execute(object parameter)
        {
            if (!CanExecute(parameter))
            {
                throw new InvalidOperationException("Execute() invoked when it cannot execute.");
            }
            await _executeMethodAsync((T)parameter);
        }

        public Task ExecuteAsync(object parameter)
        {
            if (!CanExecute(parameter))
            {
                throw new InvalidOperationException("Execute() invoked when it cannot execute.");
            }
            return _executeMethodAsync((T)parameter);
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
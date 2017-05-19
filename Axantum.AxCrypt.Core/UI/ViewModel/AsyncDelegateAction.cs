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

        private Func<T, Task<bool>> _canExecuteMethodAsync;

        public AsyncDelegateAction(Func<T, Task> executeMethodAsync, Func<T, Task<bool>> canExecuteMethodAsync)
        {
            if (executeMethodAsync == null)
            {
                throw new ArgumentNullException(nameof(executeMethodAsync));
            }
            if (canExecuteMethodAsync == null)
            {
                throw new ArgumentNullException(nameof(canExecuteMethodAsync));
            }

            _executeMethodAsync = executeMethodAsync;
            _canExecuteMethodAsync = canExecuteMethodAsync;
        }

        public AsyncDelegateAction(Func<T, Task> executeMethodAsync)
            : this(executeMethodAsync, (parameter) => Task.FromResult(true))
        {
        }

        public Task<bool> CanExecuteAsync(object parameter)
        {
            return _canExecuteMethodAsync(parameter != null ? (T)parameter : default(T));
        }

        public async Task ExecuteAsync(object parameter)
        {
            if (!await CanExecuteAsync(parameter))
            {
                throw new InvalidOperationException("Execute() invoked when it cannot execute.");
            }
            await _executeMethodAsync((T)parameter);
        }

        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            EventHandler handler = CanExecuteChanged;
            if (handler != null)
            {
                Resolve.UIThread.SendTo(() => handler(this, new EventArgs()));
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "Method name taken from DelegateCommand implementation by Microsoft Patterns & Practices.")]
        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged();
        }

        public bool CanExecute(object parameter)
        {
            throw new NotImplementedException();
        }

        public void Execute(object parameter)
        {
            throw new NotImplementedException();
        }
    }
}
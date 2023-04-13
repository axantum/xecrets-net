using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Core.UI.ViewModel
{
    public class AsyncDelegateAction<T> : IAsyncAction
    {
        private readonly Func<T, Task> _executeMethodAsync;

        private readonly Func<T?, Task<bool>> _canExecuteMethodAsync;

        public AsyncDelegateAction(Func<T, Task> executeMethodAsync, Func<T?, Task<bool>> canExecuteMethodAsync)
        {
            _executeMethodAsync = executeMethodAsync ?? throw new ArgumentNullException(nameof(executeMethodAsync));
            _canExecuteMethodAsync = canExecuteMethodAsync ?? throw new ArgumentNullException(nameof(canExecuteMethodAsync));
        }

        public AsyncDelegateAction(Func<T, Task> executeMethodAsync)
            : this(executeMethodAsync, (parameter) => Task.FromResult(true))
        {
        }

        public Task<bool> CanExecuteAsync(object parameter)
        {
            return _canExecuteMethodAsync(parameter != null ? (T)parameter : default);
        }

        public async Task ExecuteAsync(object parameter)
        {
            if (!await CanExecuteAsync(parameter))
            {
                throw new InvalidOperationException("Execute() invoked when it cannot execute.");
            }
            await _executeMethodAsync((T)parameter);
        }

        public event EventHandler? CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            EventHandler? handler = CanExecuteChanged;
            if (handler != null)
            {
                Resolve.UIThread.SendTo(() => handler(this, new EventArgs()));
            }
        }

        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged();
        }
    }
}

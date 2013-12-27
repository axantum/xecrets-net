using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

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
            _executeMethod = executeMethod;
            _canExecuteMethod = canExecuteMethod;
        }

        public DelegateAction(Action<T> executeMethod)
            : this(executeMethod, (parameter) => true)
        {
        }

        public bool CanExecute(object parameter)
        {
            return _canExecuteMethod((T)parameter);
        }

        public void Execute(object parameter)
        {
            if (!CanExecute(parameter))
            {
                throw new InvalidOperationException("Execute() invoked when CanExecute() is false.");
            }
            _executeMethod((T)parameter);
        }

        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            EventHandler handler = CanExecuteChanged;
            if (handler != null)
            {
                Instance.UIThread.RunOnUIThread(() => handler(this, new EventArgs()));
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "Method name taken from DelegateCommand implementation by Microsoft Patterns & Practices.")]
        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged();
        }
    }
}
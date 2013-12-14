using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    /// <summary>
    /// Defines an action on the Model exposed via a ViewModel. It is identical to the System.Windows.Input.ICommand
    /// interface, but duplicated to avoid pulling a dependency that may not always be supported.
    /// </summary>
    public interface IAction
    {
        bool CanExecute(object parameter);

        void Execute(object parameter);

        event EventHandler CanExecuteChanged;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public interface IAsyncAction
    {
        Task ExecuteAsync(object parameter);

        Task<bool> CanExecuteAsync(object parameter);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public interface IPropertyBinder : IViewModel
    {
        void BindPropertyChanged<T>(string name, Action<T> action);

        void BindPropertyAsyncChanged<T>(string name, Func<T, Task> action);
    }
}
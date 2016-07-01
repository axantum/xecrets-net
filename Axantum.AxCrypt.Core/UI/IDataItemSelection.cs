using Axantum.AxCrypt.Core.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.UI
{
    public interface IDataItemSelection
    {
        void HandleSelection(FileSelectionEventArgs e);
    }
}
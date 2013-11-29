using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.UI
{
    public interface INotify : INotifyPropertyChanged
    {
        void Notify();
    }
}

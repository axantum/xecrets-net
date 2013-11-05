using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.UI
{
    public interface IUIThread
    {
        bool IsOnUIThread { get; }

        void RunOnUIThread(Action action);
    }
}
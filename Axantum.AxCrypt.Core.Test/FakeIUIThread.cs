using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Test
{
    internal class FakeIUIThread : IUIThread
    {
        public bool IsOnUIThread
        {
            get { return false; }
        }

        public void RunOnUIThread(Action action)
        {
            action();
        }
    }
}
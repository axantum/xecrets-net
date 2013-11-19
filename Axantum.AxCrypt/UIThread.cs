using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Axantum.AxCrypt
{
    public class UIThread : IUIThread
    {
        private Control _control;

        private SynchronizationContext _context;

        public UIThread(Control control)
        {
            _control = control;
            _context = SynchronizationContext.Current;
        }

        public bool IsOnUIThread
        {
            get { return _control.InvokeRequired; }
        }

        public void RunOnUIThread(Action action)
        {
            _context.Send((state) => { action(); }, null);
        }
    }
}
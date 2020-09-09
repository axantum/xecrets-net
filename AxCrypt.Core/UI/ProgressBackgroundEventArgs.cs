using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Core.UI
{
    public class ProgressBackgroundEventArgs : EventArgs
    {
        public ProgressBackgroundEventArgs(IProgressContext progressContext)
        {
            ProgressContext = progressContext;
        }

        public IProgressContext ProgressContext { get; private set; }

        public object State { get; set; }
    }
}
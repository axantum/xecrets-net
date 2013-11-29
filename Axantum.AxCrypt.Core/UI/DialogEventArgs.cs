using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.UI
{
    public class DialogEventArgs : EventArgs
    {
        public DialogEventArgs()
        {
            Responses = new List<string>();
        }

        public bool Cancel { get; set; }

        public IList<string> Responses { get; private set; }
    }
}

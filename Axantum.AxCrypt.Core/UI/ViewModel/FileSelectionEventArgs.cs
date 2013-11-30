using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public class FileSelectionEventArgs: EventArgs
    {
        public FileSelectionEventArgs()
        {
            SelectedFiles = new List<string>();
        }

        public FileSelectionType FileSelectionType { get; set; }

        public bool Cancel { get; set; }

        public bool ConfirmAll { get; set; }

        public bool Skip { get; set; }

        public IList<string> SelectedFiles { get; set; }
    }
}

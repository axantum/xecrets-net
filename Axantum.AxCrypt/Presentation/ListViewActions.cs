using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Axantum.AxCrypt.Presentation
{
    public class ListViewActions
    {
        private ListView _listView;

        public ListViewActions(ListView listView)
        {
            _listView = listView;
        }

        public void ShowContextMenu(ToolStripDropDown contextMenu, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }
            contextMenu.Show(_listView, e.Location);
        }
    }
}
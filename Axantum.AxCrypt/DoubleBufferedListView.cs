using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Axantum.AxCrypt
{
    public class DoubleBufferedListView : ListView
    {
        public DoubleBufferedListView()
        {
            DoubleBuffered = true;
        }
    }
}
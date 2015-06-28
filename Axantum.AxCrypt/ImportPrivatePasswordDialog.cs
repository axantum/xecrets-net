using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Axantum.AxCrypt
{
    public partial class ImportPrivatePasswordDialog : Form
    {
        public ImportPrivatePasswordDialog()
        {
            InitializeComponent();
            new Styling().Style(this);
        }
    }
}
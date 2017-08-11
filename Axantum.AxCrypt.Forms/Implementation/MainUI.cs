using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Axantum.AxCrypt.Forms.Implementation
{
    public class MainUI : IMainUI
    {
        private Form _mainForm;

        public MainUI(Form mainForm)
        {
            _mainForm = mainForm;
        }

        public bool Enabled
        {
            get
            {
                return _mainForm.Enabled;
            }

            set
            {
                _mainForm.Enabled = value;
            }
        }
    }
}
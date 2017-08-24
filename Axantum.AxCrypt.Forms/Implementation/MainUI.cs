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
        private Stack<bool> _states = new Stack<bool>();

        private Form _mainForm;

        public MainUI(Form mainForm)
        {
            _mainForm = mainForm;
        }

        public void DisableUI()
        {
            _states.Push(_mainForm.Enabled);
            _mainForm.Enabled = false;
        }

        public void RestoreUI()
        {
            _mainForm.Enabled = _states.Pop();
        }
    }
}
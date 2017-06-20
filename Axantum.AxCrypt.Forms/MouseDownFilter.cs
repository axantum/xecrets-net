using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Axantum.AxCrypt.Forms
{
    public class MouseDownFilter : IMessageFilter
    {
        public event EventHandler FormClicked;
        private int WM_LBUTTONDOWN = 0x201;
        private Form _form = null;

        [DllImport("user32.dll")]
        public static extern bool IsChild(IntPtr hWndParent, IntPtr hWnd);

        public MouseDownFilter(Form form)
        {
            _form = form;
        }

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_LBUTTONDOWN)
            {
                if (Form.ActiveForm != null && Form.ActiveForm.Equals(_form))
                {
                    OnFormClicked();
                }
            }
            return false;
        }

        protected void OnFormClicked()
        {
            if (FormClicked != null)
            {
                FormClicked(_form, EventArgs.Empty);
            }
        }
    }
}

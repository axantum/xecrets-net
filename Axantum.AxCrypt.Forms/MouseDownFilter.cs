using System;
using System.Windows.Forms;

namespace Axantum.AxCrypt.Forms
{
    public class MouseDownFilter : IMessageFilter, IDisposable
    {
        public event EventHandler FormClicked;
        private int WM_LBUTTONDOWN = 0x201;
        private Form _form = null;

        public MouseDownFilter(Form form)
        {
            _form = form;
            Application.AddMessageFilter(this);
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
            FormClicked?.Invoke(_form, EventArgs.Empty);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                return;
            }
            Application.RemoveMessageFilter(this);
        }
    }
}

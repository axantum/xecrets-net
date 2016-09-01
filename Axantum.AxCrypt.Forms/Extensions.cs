using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Axantum.AxCrypt.Forms
{
    public static class Extensions
    {
        public static async Task WithWaitCursorAsync(this Control control, Func<Task> action, Action final)
        {
            try
            {
                control.UseWaitCursor = true;
                await action();
            }
            finally
            {
                control.UseWaitCursor = false;
                final();
            }
        }

        public static Task<DialogResult> ShowDialogAsync(this Form self, Form parent)
        {
            if (self == null)
            {
                throw new ArgumentNullException("self");
            }

            TaskCompletionSource<DialogResult> completion = new TaskCompletionSource<DialogResult>();
            self.BeginInvoke(new Action(() => completion.SetResult(self.ShowDialog(parent))));

            return completion.Task;
        }
    }
}
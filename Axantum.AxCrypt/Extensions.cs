using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Axantum.AxCrypt
{
    internal static class Extensions
    {
        public static void ShowWarning(this string message)
        {
            MessageBox.Show(message, "AxCypt", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, AxCryptMainForm.MessageBoxOptions);
        }
    }
}
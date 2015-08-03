using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Forms
{
    internal static class NativeMethods
    {
        [DllImport("gdi32.dll")]
        public static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont, IntPtr pdv, [In] ref uint pcFonts);
    }
}
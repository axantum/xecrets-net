using AxCrypt.Core.Runtime;
using AxCrypt.Desktop.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Desktop
{
    public class FontLoader : IDisposable
    {
        private PrivateFontCollection _privateFontCollection1 = new PrivateFontCollection();

        private PrivateFontCollection _privateFontCollection2 = new PrivateFontCollection();

        private static bool IsWindowsDesktop
        {
            get
            {
                return New<IPlatform>().Platform == Platform.WindowsDesktop;
            }
        }

        public FontLoader()
        {
            if (!IsWindowsDesktop)
            {
                return;
            }
            AddFontFromResource(_privateFontCollection1, Resources.OpenSans_Light);
            AddFontFromResource(_privateFontCollection1, Resources.OpenSans_Regular);
            AddFontFromResource(_privateFontCollection1, Resources.OpenSans_Semibold);
            AddFontFromResource(_privateFontCollection2, Resources.OpenSans_Bold);
        }

        public Font ContentText
        {
            get
            {
                if (!IsWindowsDesktop)
                {
                    return null;
                }
                return new Font(_privateFontCollection1.Families[0], 10, FontStyle.Regular);
            }
        }

        public Font PromptText
        {
            get
            {
                if (!IsWindowsDesktop)
                {
                    return null;
                }
                return new Font(_privateFontCollection2.Families[0], 9, FontStyle.Bold);
            }
        }

        private static void AddFontFromResource(PrivateFontCollection privateFontCollection, byte[] fontBytes)
        {
            var fontData = Marshal.AllocCoTaskMem(fontBytes.Length);
            Marshal.Copy(fontBytes, 0, fontData, fontBytes.Length);

            uint cFonts = 0;
            NativeMethods.AddFontMemResourceEx(fontData, (uint)fontBytes.Length, IntPtr.Zero, ref cFonts);

            privateFontCollection.AddMemoryFont(fontData, fontBytes.Length);
            Marshal.FreeCoTaskMem(fontData);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (_privateFontCollection1 != null)
            {
                _privateFontCollection1.Dispose();
                _privateFontCollection1 = null;
            }
            if (_privateFontCollection2 != null)
            {
                _privateFontCollection2.Dispose();
                _privateFontCollection2 = null;
            }
        }
    }
}
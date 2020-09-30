using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AxCrypt.Desktop
{
    internal static class NativeMethods
    {
        [DllImport("gdi32.dll")]
        internal static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont, IntPtr pdv, [In] ref uint pcFonts);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SystemParametersInfo(uint uAction, uint uParam, ref bool lpvParam, int fWinIni);

        [DllImport("shell32.dll")]
        public static extern void SHChangeNotify(HChangeNotifyEventID wEventId, HChangeNotifyFlags uFlags, IntPtr dwItem1, IntPtr dwItem2);

        [Flags]
        public enum HChangeNotifyEventID
        {
            SHCNE_ALLEVENTS = 0x7FFFFFFF,
            SHCNE_ASSOCCHANGED = 0x08000000,
            SHCNE_UPDATEDIR = 0x00001000,
        }

        [Flags]
        public enum HChangeNotifyFlags
        {
            SHCNF_DWORD = 0x0003,
            SHCNF_IDLIST = 0x0000,
        }

        [DllImport("wininet.dll", SetLastError = true)]
        public static extern bool InternetGetConnectedState(out int lpdwFlags, int dwReserved);

        [Flags]
        private enum ConnectionStates
        {
            Modem = 0x1,
            LAN = 0x2,
            Proxy = 0x4,
            RasInstalled = 0x10,
            Offline = 0x20,
            Configured = 0x40,
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);

        private delegate uint IsInternetConnectedDelegate();

        public static bool IsInternetConnected()
        {
            IntPtr hModule = NativeMethods.LoadLibrary(@"connect.dll");
            if (hModule == IntPtr.Zero)
            {
                return true;
            }

            try
            {
                IntPtr pointerToIsInternetConnected = NativeMethods.GetProcAddress(hModule, @"IsInternetConnected");
                if (pointerToIsInternetConnected == IntPtr.Zero)
                {
                    return true;
                }

                IsInternetConnectedDelegate isInternetConnected = (IsInternetConnectedDelegate)Marshal.GetDelegateForFunctionPointer(pointerToIsInternetConnected, typeof(IsInternetConnectedDelegate));
                uint hResult = isInternetConnected();

                if (hResult == 0)
                {
                    return true;
                }

                return false;
            }
            finally
            {
                NativeMethods.FreeLibrary(hModule);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "4")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "3")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "2")]
        [DllImport("Shlwapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern uint AssocQueryString(ASSOCF flags, ASSOCSTR str, string pszAssoc, string pszExtra, [Out] StringBuilder pszOut, [In][Out] ref uint pcchOut);

        [Flags]
        public enum ASSOCF
        {
            ASSOCF_NONE = 0x00000000,
            ASSOCF_INIT_NOREMAPCLSID = 0x00000001,
            ASSOCF_INIT_BYEXENAME = 0x00000002,
            ASSOCF_OPEN_BYEXENAME = 0x00000002,
            ASSOCF_INIT_DEFAULTTOSTAR = 0x00000004,
            ASSOCF_INIT_DEFAULTTOFOLDER = 0x00000008,
            ASSOCF_NOUSERSETTINGS = 0x00000010,
            ASSOCF_NOTRUNCATE = 0x00000020,
            ASSOCF_VERIFY = 0x00000040,
            ASSOCF_REMAPRUNDLL = 0x00000080,
            ASSOCF_NOFIXUPS = 0x00000100,
            ASSOCF_IGNOREBASECLASS = 0x00000200,
            ASSOCF_INIT_IGNOREUNKNOWN = 0x00000400,
            ASSOCF_INIT_FIXED_PROGID = 0x00000800,
            ASSOCF_IS_PROTOCOL = 0x00001000,
            ASSOCF_INIT_FOR_FILE = 0x00002000,
        }

        public enum ASSOCSTR
        {
            ASSOCSTR_COMMAND = 1,
            ASSOCSTR_EXECUTABLE,
            ASSOCSTR_FRIENDLYDOCNAME,
            ASSOCSTR_FRIENDLYAPPNAME,
            ASSOCSTR_NOOPEN,
            ASSOCSTR_SHELLNEWVALUE,
            ASSOCSTR_DDECOMMAND,
            ASSOCSTR_DDEIFEXEC,
            ASSOCSTR_DDEAPPLICATION,
            ASSOCSTR_DDETOPIC,
            ASSOCSTR_INFOTIP,
            ASSOCSTR_QUICKTIP,
            ASSOCSTR_TILEINFO,
            ASSOCSTR_CONTENTTYPE,
            ASSOCSTR_DEFAULTICON,
            ASSOCSTR_SHELLEXTENSION,
            ASSOCSTR_DROPTARGET,
            ASSOCSTR_DELEGATEEXECUTE,
            ASSOCSTR_SUPPORTED_URI_PROTOCOLS,
            ASSOCSTR_PROGID,
            ASSOCSTR_APPID,
            ASSOCSTR_APPPUBLISHER,
            ASSOCSTR_APPICONREFERENCE,
            ASSOCSTR_MAX,
        }

        [DllImport("User32", SetLastError = true, EntryPoint = "RegisterPowerSettingNotification", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid, Int32 Flags);

        [DllImport("User32", EntryPoint = "UnregisterPowerSettingNotification", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnregisterPowerSettingNotification(IntPtr handle);

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        internal struct POWERBROADCAST_SETTING
        {
            public Guid PowerSetting;
            public Int32 DataLength;
        }

        public static Guid GUID_MONITOR_POWER_ON = new Guid("02731015-4510-4526-99e6-e5a17ebd1aea");
        public static Guid GUID_CONSOLE_DISPLAY_STATE = new Guid("6fe69556-704a-47a0-8f24-c28d936fda47");

        public const int WM_POWERBROADCAST = 0x0218;

        public const int PBT_POWERSETTINGCHANGE = 0x8013;

        public const int DEVICE_NOTIFY_WINDOW_HANDLE = 0;
    }
}
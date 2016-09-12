using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Axantum.AxCrypt.Forms
{
    internal static class NativeMethods
    {
        [DllImport("gdi32.dll")]
        public static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont, IntPtr pdv, [In] ref uint pcFonts);

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

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SystemParametersInfo(uint uAction, uint uParam, ref bool lpvParam, int fWinIni);

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
    }
}
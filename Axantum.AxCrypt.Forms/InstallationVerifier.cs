using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Axantum.AxCrypt.Core.Runtime;
using Microsoft.Win32;
using static Axantum.AxCrypt.Abstractions.TypeResolve;


namespace Axantum.AxCrypt.Forms
{
    public class InstallationVerifier
    {
        private readonly Lazy<bool> _isFileAssociationOk = new Lazy<bool>(IsFileAssociationCorrect);
        private readonly Lazy<bool> _isApplicationInstalled = new Lazy<bool>(CheckApplicationInstallationState);

        /// <summary>
        /// This is part of #265 Check and re-assert the ".axx" file name association.
        /// 
        /// Use the AssocQueryString in shlwapi.dll . We already do a little P/Invoke in Axantum.AxCrypt.Forms NativeMethods.cs so that's fine to use.
        /// This will be called from the main program and popping up the appropriate warning. We can then add other properties there. 
        /// (Such as "HasBrokenWebCompanion" which is needed for another issue).
        ///
        /// After analysis we have identify that windows store the file association for user selection if any under 
        /// Computer\HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\Roaming\OpenWith\FileExts
        ///
        /// </summary>
        public bool IsFileAssociationOk => _isFileAssociationOk.Value;

        public bool IsApplicationInstalled => _isApplicationInstalled.Value;

        private static bool IsFileAssociationCorrect()
        {
            uint pcchOut = 0;
            NativeMethods.AssocQueryString(NativeMethods.ASSOCF.ASSOCF_VERIFY,
                NativeMethods.ASSOCSTR.ASSOCSTR_EXECUTABLE, New<IRuntimeEnvironment>().AxCryptExtension, null, null, ref pcchOut);

            StringBuilder pszOut = new StringBuilder((int)pcchOut);
            NativeMethods.AssocQueryString(NativeMethods.ASSOCF.ASSOCF_VERIFY,
                NativeMethods.ASSOCSTR.ASSOCSTR_EXECUTABLE, New<IRuntimeEnvironment>().AxCryptExtension, null, pszOut, ref pcchOut);

            return String.Equals(Environment.GetCommandLineArgs()[0], pszOut.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private static bool CheckApplicationInstallationState()
        {
            string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using (Microsoft.Win32.RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey))
            {
                if (key == null)
                {
                    return false;
                }
                foreach (string subKeyName in key.GetSubKeyNames())
                {
                    using (RegistryKey subKey = key.OpenSubKey(subKeyName))
                    {
                        if (subKey?.GetValue("DisplayName")?.ToString().StartsWith("AxCrypt") ?? false)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AxCrypt.Core.Runtime;
using Microsoft.Win32;
using static AxCrypt.Abstractions.TypeResolve;
using AxCrypt.Core.IO;
using System.IO;

namespace AxCrypt.Desktop
{
    public class InstallationVerifier
    {
        private readonly Lazy<bool> _isFileAssociationOk = new Lazy<bool>(IsFileAssociationCorrect);

        private readonly Lazy<bool> _isApplicationInstalled = new Lazy<bool>(CheckApplicationInstallationState);

        private readonly Lazy<bool> _isLavasoftApplicationInstalled = new Lazy<bool>(CheckLavasofApplicationInstallationState);

        /// <summary>
        /// Gets a value indicating whether this instance is application installed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the file extension association is ok; otherwise, <c>false</c>.
        /// </value>
        public bool IsFileAssociationOk => _isFileAssociationOk.Value;

        /// <summary>
        /// Gets a value indicating whether AxCrypt is installed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if AxCrypt is installed; otherwise, <c>false</c>.
        /// </value>
        public bool IsApplicationInstalled => _isApplicationInstalled.Value;

        /// <summary>
        /// Gets a value indicating whether we found Lavasoft Web Companion to be installed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if lavasoft was found; otherwise, <c>false</c>.
        /// </value>
        public bool IsLavasoftApplicationInstalled => _isLavasoftApplicationInstalled.Value;

        /// <summary>
        /// Using AssocQueryString in shlwapi.dll determine if the .axx file association is correct.
        ///
        /// (Another possibility is to check the file association for user selection if any under
        /// Computer\HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\Roaming\OpenWith\FileExts)
        /// </summary>
        private static bool IsFileAssociationCorrect()
        {
            uint pcchOut = 0;
            NativeMethods.AssocQueryString(NativeMethods.ASSOCF.ASSOCF_VERIFY,
                NativeMethods.ASSOCSTR.ASSOCSTR_EXECUTABLE, New<IRuntimeEnvironment>().AxCryptExtension, null, null, ref pcchOut);

            StringBuilder pszOut = new StringBuilder((int)pcchOut);
            NativeMethods.AssocQueryString(NativeMethods.ASSOCF.ASSOCF_VERIFY,
                NativeMethods.ASSOCSTR.ASSOCSTR_EXECUTABLE, New<IRuntimeEnvironment>().AxCryptExtension, null, pszOut, ref pcchOut);

            string associatedProgramFilePath = pszOut.ToString();
            string axCryptProgramFilePath = Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles"), "AxCrypt", "AxCrypt", "AxCrypt.exe");
            return String.Equals(axCryptProgramFilePath, associatedProgramFilePath, StringComparison.OrdinalIgnoreCase);
        }

        private static bool CheckApplicationInstallationState()
        {
            string registryKey = @"SOFTWARE\AxCrypt\AxCrypt";
            using (Microsoft.Win32.RegistryKey key = Registry.CurrentUser.OpenSubKey(registryKey))
            {
                if (key == null)
                {
                    return false;
                }
                int? installed = key.GetValue("installed", 0) as int?;

                return installed.GetValueOrDefault() == 1;
            }
        }

        private static bool CheckLavasofApplicationInstallationState()
        {
            string path = Path.Combine(Environment.GetEnvironmentVariable("windir"), "System32", "LavasoftTcpService64.dll");
            return File.Exists(path);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Axantum.AxCrypt.Forms
{
    public class InstallationVerifier
    {
        private bool? _isFileAssociationOk;


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
        public bool IsFileAssociationOk
        {
            get
            {
                if (!_isFileAssociationOk.HasValue)
                {
                    _isFileAssociationOk = IsFileAssociationCorrect();
                }
                return _isFileAssociationOk.Value;
            }
        }

        private bool IsFileAssociationCorrect()
        { 
            uint pcchOut = 0;
            NativeMethods.AssocQueryString(NativeMethods.ASSOCF.ASSOCF_VERIFY,
                NativeMethods.ASSOCSTR.ASSOCSTR_EXECUTABLE, ".axx", null, null, ref pcchOut);

            StringBuilder pszOut = new StringBuilder((int)pcchOut);
            NativeMethods.AssocQueryString(NativeMethods.ASSOCF.ASSOCF_VERIFY,
                NativeMethods.ASSOCSTR.ASSOCSTR_EXECUTABLE, ".axx", null, pszOut, ref pcchOut);

            if (Application.ExecutablePath == pszOut.ToString())
            {
                return true;
            }
                
             return false;
        }

    }
}

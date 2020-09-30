#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
 *
 * This file is part of AxCrypt.
 *
 * AxCrypt is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * AxCrypt is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AxCrypt.  If not, see <http://www.gnu.org/licenses/>.
 *
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AxCrypt.Mono
{
    public class MonoPlatform : IPlatform
    {
        private Lazy<bool> isMac = new Lazy<bool>(IsMac);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static bool IsMac()
        {
            try
            {
                using (Process p = Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = "uname",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                    }
                ))
                {
                    string output = p.StandardOutput.ReadToEnd().Trim();
                    return output == "Darwin";
                }
            }
            catch
            {
                return false;
            }
        }

        public Platform Platform
        {
            get
            {
                OperatingSystem os = global::System.Environment.OSVersion;
                PlatformID pid = os.Platform;
                switch (pid)
                {
                    case PlatformID.Win32NT:
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                        return Platform.WindowsDesktop;

                    case PlatformID.MacOSX:
                        return Platform.MacOsx;

                    case PlatformID.Unix:
                        return isMac.Value ? Platform.MacOsx : Platform.Linux;

                    case PlatformID.WinCE:
                        return Platform.WindowsMobile;

                    case PlatformID.Xbox:
                        return Platform.Xbox;

                    default:
                        return Platform.Unknown;
                }
            }
        }
    }
}
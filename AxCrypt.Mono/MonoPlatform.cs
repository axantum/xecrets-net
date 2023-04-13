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

using System.Diagnostics;

namespace AxCrypt.Mono
{
    public class MonoPlatform : IPlatform
    {
        private readonly Lazy<bool> isMac = new Lazy<bool>(IsMac);

        private static bool IsMac()
        {
            try
            {   
                using Process? p = Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = "uname",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                    }
                );
                string? output = p?.StandardOutput.ReadToEnd().Trim();
                return output == "Darwin";
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
                OperatingSystem os = Environment.OSVersion;
                PlatformID pid = os.Platform;
                return pid switch
                {
                    PlatformID.Win32NT or PlatformID.Win32S or PlatformID.Win32Windows => Platform.WindowsDesktop,
                    PlatformID.MacOSX => Platform.MacOsx,
                    PlatformID.Unix => isMac.Value ? Platform.MacOsx : Platform.Linux,
                    PlatformID.WinCE => Platform.WindowsMobile,
                    PlatformID.Xbox => Platform.Xbox,
                    _ => Platform.Unknown,
                };
            }
        }
    }
}

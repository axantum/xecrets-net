#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using System;
using System.IO;
using System.Security.Cryptography;
using Axantum.AxCrypt.Core.IO;

namespace Axantum.AxCrypt.Core.System
{
    public class RuntimeEnvironment : IRuntimeEnvironment
    {
        public RuntimeEnvironment()
            : this(".axx")
        {
        }

        public RuntimeEnvironment(string extension)
        {
            AxCryptExtension = extension;
        }

        public bool IsLittleEndian
        {
            get
            {
                return BitConverter.IsLittleEndian;
            }
        }

        private RandomNumberGenerator _rng;

        public byte[] GetRandomBytes(int count)
        {
            if (_rng == null)
            {
                _rng = RandomNumberGenerator.Create();
            }

            byte[] data = new byte[count];
            _rng.GetBytes(data);
            return data;
        }

        public IRuntimeFileInfo FileInfo(FileInfo file)
        {
            return new RuntimeFileInfo(file);
        }

        public IRuntimeFileInfo FileInfo(string path)
        {
            return FileInfo(new FileInfo(path));
        }

        public string AxCryptExtension
        {
            get;
            set;
        }

        public bool IsDesktopWindows
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
                        return true;
                    case PlatformID.MacOSX:
                    case PlatformID.Unix:
                    case PlatformID.WinCE:
                    case PlatformID.Xbox:
                    default:
                        return false;
                }
            }
        }

        public int StreamBufferSize
        {
            get { return 65536; }
        }
    }
}
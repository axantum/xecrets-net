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
using System.Diagnostics.CodeAnalysis;
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

        public IRuntimeFileInfo FileInfo(string path)
        {
            return new RuntimeFileInfo(path);
        }

        public string AxCryptExtension
        {
            get;
            set;
        }

        public Platform Platform
        {
            [ExcludeFromCodeCoverage]
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
                        return Platform.Linux;
                    case PlatformID.WinCE:
                        return Platform.WindowsMobile;
                    case PlatformID.Xbox:
                        return Platform.Xbox;
                    default:
                        return Platform.Unknown;
                }
            }
        }

        public int StreamBufferSize
        {
            get { return 65536; }
        }

        public IFileWatcher FileWatcher(string path)
        {
            return new FileWatcher(path);
        }

        private IRuntimeFileInfo _temporaryDirectoryInfo;

        public IRuntimeFileInfo TemporaryDirectoryInfo
        {
            get
            {
                if (_temporaryDirectoryInfo == null)
                {
                    string temporaryFolderPath = Path.Combine(Path.GetTempPath(), @"AxCrypt" + Path.DirectorySeparatorChar);
                    IRuntimeFileInfo temporaryFolderInfo = FileInfo(temporaryFolderPath);
                    temporaryFolderInfo.CreateDirectory();
                    _temporaryDirectoryInfo = temporaryFolderInfo;
                }

                return _temporaryDirectoryInfo;
            }
        }

        public DateTime UtcNow
        {
            get { return DateTime.UtcNow; }
        }

        [ExcludeFromCodeCoverage]
        public ILauncher Launch(string path)
        {
            return new Launcher(path);
        }

        public ITiming StartTiming()
        {
            return new Timing();
        }

        [ExcludeFromCodeCoverage]
        public IWebCaller CreateWebCaller()
        {
            return new WebCaller();
        }

        public void NotifyFileChanged()
        {
            OnChanged();
        }

        protected virtual void OnChanged()
        {
            EventHandler<EventArgs> handler = FileChanged;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        public event EventHandler<EventArgs> FileChanged;
    }
}
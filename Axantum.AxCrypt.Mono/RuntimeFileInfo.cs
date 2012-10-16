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
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.IO;

namespace Axantum.AxCrypt.Mono
{
    internal class RuntimeFileInfo : IRuntimeFileInfo
    {
        private FileInfo _file;

        public RuntimeFileInfo(string fullName)
        {
            if (fullName == null)
            {
                throw new ArgumentNullException("fullName");
            }
            _file = new FileInfo(fullName);
        }

        public Stream OpenRead()
        {
            return new FileStream(_file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, OS.Current.StreamBufferSize);
        }

        public Stream OpenWrite()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_file.FullName));
            return new FileStream(_file.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, OS.Current.StreamBufferSize);
        }

        public string Name
        {
            get
            {
                return _file.Name;
            }
        }

        public DateTime CreationTimeUtc
        {
            get
            {
                _file.Refresh();
                return _file.CreationTimeUtc;
            }
            set
            {
                _file.CreationTimeUtc = value;
            }
        }

        public DateTime LastAccessTimeUtc
        {
            get
            {
                _file.Refresh();
                return _file.LastAccessTimeUtc;
            }
            set
            {
                _file.LastAccessTimeUtc = value;
            }
        }

        public DateTime LastWriteTimeUtc
        {
            get
            {
                _file.Refresh();
                return _file.LastWriteTimeUtc;
            }
            set
            {
                _file.LastWriteTimeUtc = value;
            }
        }

        public void SetFileTimes(DateTime creationTimeUtc, DateTime lastAccessTimeUtc, DateTime lastWriteTimeUtc)
        {
            CreationTimeUtc = creationTimeUtc;
            LastAccessTimeUtc = lastAccessTimeUtc;
            LastWriteTimeUtc = lastWriteTimeUtc;
        }

        public IRuntimeFileInfo CreateEncryptedName()
        {
            string encryptedName = _file.FullName.CreateEncryptedName();

            return new RuntimeFileInfo(encryptedName);
        }

        public string FullName
        {
            get { return _file.FullName; }
        }

        public bool Exists
        {
            get
            {
                _file.Refresh();
                return _file.Exists;
            }
        }

        public void MoveTo(string destinationFileName)
        {
            _file.MoveTo(destinationFileName);
        }

        public void Delete()
        {
            _file.Delete();
        }

        #region IRuntimeFileInfo Members

        public void CreateDirectory()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_file.FullName));
        }

        #endregion IRuntimeFileInfo Members
    }
}
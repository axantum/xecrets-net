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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Axantum.AxCrypt.Core.IO;

namespace Axantum.AxCrypt.Core.Test
{
    internal class FakeRuntimeFileInfo : IRuntimeFileInfo
    {
        private class FakeFileInfo
        {
            public string FullName;
            public DateTime CreationTimeUtc;
            public DateTime LastAccessTimeUtc;
            public DateTime LastWriteTimeUtc;
            public Stream Stream;
        }

        public static readonly DateTime TestDate1Utc = DateTime.Parse("2012-01-02 03:04:05", CultureInfo.GetCultureInfo("sv-SE"), DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        public static readonly DateTime TestDate2Utc = DateTime.Parse("1950-12-24 15:16:17", CultureInfo.GetCultureInfo("sv-SE"), DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        public static readonly DateTime TestDate3Utc = DateTime.Parse("2100-12-31 00:00:00", CultureInfo.GetCultureInfo("sv-SE"), DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        public static readonly DateTime TestDate4Utc = DateTime.Parse("2008-09-10 11:12:13", CultureInfo.GetCultureInfo("sv-SE"), DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        public static readonly DateTime TestDate5Utc = DateTime.Parse("2009-03-31 06:07:08", CultureInfo.GetCultureInfo("sv-SE"), DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        public static readonly DateTime TestDate6Utc = DateTime.Parse("2012-02-29 12:00:00", CultureInfo.GetCultureInfo("sv-SE"), DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);

        private static Dictionary<string, FakeFileInfo> _fakeFileSystem = new Dictionary<string, FakeFileInfo>(StringComparer.OrdinalIgnoreCase);

        private FakeFileInfo _file;

        public static event EventHandler OpeningForRead;

        public static event EventHandler OpeningForWrite;

        public static event EventHandler Deleting;

        public static void AddFile(string path, DateTime creationTimeUtc, DateTime lastAccessTimeUtc, DateTime lastWriteTimeUtc, Stream stream)
        {
            FakeFileInfo fileInfo = new FakeFileInfo { FullName = path, CreationTimeUtc = creationTimeUtc, LastAccessTimeUtc = lastAccessTimeUtc, LastWriteTimeUtc = lastWriteTimeUtc, Stream = stream };
            _fakeFileSystem.Add(path, fileInfo);
            ((FakeRuntimeEnvironment)OS.Current).FileCreated(path);
        }

        public static void AddFile(string path, Stream stream)
        {
            AddFile(path, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, stream);
        }

        public static void ClearFiles()
        {
            _fakeFileSystem.Clear();
        }

        public FakeRuntimeFileInfo(string fullName)
        {
            DateTime utcNow = OS.Current.UtcNow;
            _file = new FakeFileInfo { FullName = fullName, CreationTimeUtc = utcNow, LastAccessTimeUtc = utcNow, LastWriteTimeUtc = utcNow, Stream = Stream.Null };
        }

        protected virtual void OnOpeningForRead()
        {
            EventHandler handler = OpeningForRead;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        protected virtual void OnOpeningForWrite()
        {
            EventHandler handler = OpeningForWrite;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        protected virtual void OnDeleting()
        {
            EventHandler handler = Deleting;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        private FakeFileInfo FindFileInfo()
        {
            FakeFileInfo fakeFileInfo;
            if (!_fakeFileSystem.TryGetValue(_file.FullName, out fakeFileInfo))
            {
                return null;
            }
            return fakeFileInfo;
        }

        private static void EnsureDateTimes(FakeFileInfo fakeFileInfo)
        {
            DateTime utcNow = DateTime.UtcNow;
            if (fakeFileInfo.CreationTimeUtc == DateTime.MinValue)
            {
                fakeFileInfo.CreationTimeUtc = utcNow;
            }
            if (fakeFileInfo.LastAccessTimeUtc == DateTime.MinValue)
            {
                fakeFileInfo.LastAccessTimeUtc = utcNow;
            }
            if (fakeFileInfo.LastWriteTimeUtc == DateTime.MinValue)
            {
                fakeFileInfo.LastWriteTimeUtc = utcNow;
            }
        }

        public Stream OpenRead()
        {
            FakeFileInfo fakeFileInfo = FindFileInfo();
            if (fakeFileInfo == null)
            {
                throw new FileNotFoundException("Can't find '{0}'.".InvariantFormat(_file.FullName));
            }
            OnOpeningForRead();
            fakeFileInfo.Stream.Position = 0;
            EnsureDateTimes(fakeFileInfo);
            return new NonClosingStream(fakeFileInfo.Stream);
        }

        public Stream OpenWrite()
        {
            FakeFileInfo fakeFileInfo = FindFileInfo();
            if (fakeFileInfo == null)
            {
                AddFile(_file.FullName, new MemoryStream());
                _file = fakeFileInfo = FindFileInfo();
            }
            OnOpeningForWrite();
            fakeFileInfo.Stream.Position = 0;
            EnsureDateTimes(fakeFileInfo);
            return new NonClosingStream(fakeFileInfo.Stream);
        }

        public string Name
        {
            get
            {
                return Path.GetFileName(_file.FullName);
            }
        }

        public DateTime CreationTimeUtc
        {
            get
            {
                FakeFileInfo fakeFileInfo = FindFileInfo();
                if (fakeFileInfo == null)
                {
                    return DateTime.MinValue;
                }
                return fakeFileInfo.CreationTimeUtc;
            }
            set
            {
                FakeFileInfo fakeFileInfo = FindFileInfo();
                fakeFileInfo.CreationTimeUtc = value;
            }
        }

        public DateTime LastAccessTimeUtc
        {
            get
            {
                FakeFileInfo fakeFileInfo = FindFileInfo();
                if (fakeFileInfo == null)
                {
                    return DateTime.MinValue;
                }
                return fakeFileInfo.LastAccessTimeUtc;
            }
            set
            {
                FakeFileInfo fakeFileInfo = FindFileInfo();
                fakeFileInfo.LastAccessTimeUtc = value;
            }
        }

        public DateTime LastWriteTimeUtc
        {
            get
            {
                FakeFileInfo fakeFileInfo = FindFileInfo();
                if (fakeFileInfo == null)
                {
                    return DateTime.MinValue;
                }
                return fakeFileInfo.LastWriteTimeUtc;
            }
            set
            {
                FakeFileInfo fakeFileInfo = FindFileInfo();
                fakeFileInfo.LastWriteTimeUtc = value;
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

            return new FakeRuntimeFileInfo(encryptedName);
        }

        public string FullName
        {
            get { return _file.FullName; }
        }

        public bool Exists
        {
            get
            {
                return _fakeFileSystem.ContainsKey(_file.FullName);
            }
        }

        public void MoveTo(string destinationFileName)
        {
            FakeFileInfo source = _fakeFileSystem[_file.FullName];
            _fakeFileSystem.Remove(_file.FullName);

            _file = new FakeFileInfo { FullName = destinationFileName, CreationTimeUtc = source.CreationTimeUtc, LastAccessTimeUtc = source.LastAccessTimeUtc, LastWriteTimeUtc = source.LastWriteTimeUtc, Stream = source.Stream };
            _fakeFileSystem.Add(destinationFileName, _file);

            ((FakeRuntimeEnvironment)OS.Current).FileMoved(destinationFileName);
        }

        public void Delete()
        {
            OnDeleting();
            _fakeFileSystem.Remove(_file.FullName);
            ((FakeRuntimeEnvironment)OS.Current).FileDeleted(_file.FullName);
        }

        public static MemoryStream ExpandableMemoryStream(byte[] buffer)
        {
            MemoryStream stream = new MemoryStream(buffer.Length);
            stream.Write(buffer, 0, buffer.Length);
            stream.Position = 0;

            return stream;
        }

        #region IRuntimeFileInfo Members

        public void CreateDirectory()
        {
            string directory = Path.GetDirectoryName(_file.FullName);
            DateTime utcNow = DateTime.UtcNow;

            if (_fakeFileSystem.ContainsKey(directory))
            {
                return;
            }
            AddFile(directory, utcNow, utcNow, utcNow, Stream.Null);
        }

        #endregion IRuntimeFileInfo Members
    }
}
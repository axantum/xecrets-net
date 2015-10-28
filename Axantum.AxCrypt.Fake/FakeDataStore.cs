#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
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

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Fake
{
    public class FakeDataStore : IDataStore
    {
        private class FakeFileInfo
        {
            public bool IsFolder;
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

        public static event EventHandler Moving;

        public static event EventHandler OpeningForRead;

        public static event EventHandler OpeningForWrite;

        public static event EventHandler Deleting;

        public static event EventHandler ExceptionHook;

        public string TestTag { get; private set; }

        public static void AddFile(string path, bool isFolder, DateTime creationTimeUtc, DateTime lastAccessTimeUtc, DateTime lastWriteTimeUtc, Stream stream)
        {
            path = path.NormalizeFilePath();
            FakeFileInfo fileInfo = new FakeFileInfo { FullName = path, CreationTimeUtc = creationTimeUtc, LastAccessTimeUtc = lastAccessTimeUtc, LastWriteTimeUtc = lastWriteTimeUtc, Stream = stream, IsFolder = isFolder, };
            _fakeFileSystem[path] = fileInfo;

            string folder = Resolve.Portable.Path().GetDirectoryName(path);
            if (!String.IsNullOrEmpty(folder))
            {
                folder = folder.NormalizeFolderPath();
                fileInfo = new FakeFileInfo { FullName = folder, IsFolder = true, CreationTimeUtc = DateTime.MinValue, LastAccessTimeUtc = DateTime.MinValue, LastWriteTimeUtc = DateTime.MinValue, Stream = null };
                _fakeFileSystem[folder] = fileInfo;
            }

            FakeFileWatcher.HandleFileChanged(path);
        }

        public static void AddFile(string path, DateTime creationTimeUtc, DateTime lastAccessTimeUtc, DateTime lastWriteTimeUtc, Stream stream)
        {
            AddFile(path, false, creationTimeUtc, lastAccessTimeUtc, lastWriteTimeUtc, stream);
        }

        public static void AddFile(string path, Stream stream)
        {
            AddFile(path, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, stream);
        }

        public static void AddFolder(string path)
        {
            AddFile(path.NormalizeFolderPath(), true, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, null);
        }

        public static void RemoveFileOrFolder(string path)
        {
            path = path.NormalizeFilePath();
            _fakeFileSystem.Remove(path);
            FakeFileWatcher.HandleFileChanged(path);
        }

        public static void ClearFiles()
        {
            _fakeFileSystem.Clear();
        }

        public FakeDataStore(string fullName)
        {
            fullName = fullName.NormalizeFilePath();
            DateTime utcNow = OS.Current.UtcNow;
            if (_fakeFileSystem.ContainsKey(fullName))
            {
                _file = _fakeFileSystem[fullName];
                return;
            }
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

        protected virtual void OnMoving()
        {
            EventHandler handler = Moving;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        protected virtual void OnExceptionHook(string testTag)
        {
            TestTag = testTag;
            EventHandler handler = ExceptionHook;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
            TestTag = String.Empty;
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
            Stream stream = OpenUpdate();
            stream.Position = 0;
            return stream;
        }

        public Stream OpenUpdate()
        {
            FakeFileInfo fakeFileInfo = FindFileInfo();
            if (fakeFileInfo == null)
            {
                AddFile(_file.FullName, new MemoryStream());
                _file = fakeFileInfo = FindFileInfo();
            }
            OnOpeningForWrite();
            EnsureDateTimes(fakeFileInfo);
            return new NonClosingStream(fakeFileInfo.Stream);
        }

        public bool IsLocked
        {
            get
            {
                return false;
            }
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

        public string FullName
        {
            get { return _file.FullName; }
        }

        public bool IsAvailable
        {
            get
            {
                FakeFileInfo fileInfo;
                if (_fakeFileSystem.TryGetValue(_file.FullName, out fileInfo))
                {
                    return true;
                }
                return false;
            }
        }

        public void MoveTo(string destinationFileName)
        {
            destinationFileName = destinationFileName.NormalizeFilePath();
            OnMoving();
            FakeFileInfo source = _fakeFileSystem[_file.FullName];
            _fakeFileSystem.Remove(_file.FullName);

            _file = new FakeFileInfo { FullName = destinationFileName, CreationTimeUtc = source.CreationTimeUtc, LastAccessTimeUtc = source.LastAccessTimeUtc, LastWriteTimeUtc = source.LastWriteTimeUtc, Stream = source.Stream };
            _fakeFileSystem.Remove(destinationFileName);
            _fakeFileSystem.Add(destinationFileName, _file);

            FakeFileWatcher.HandleFileChanged(destinationFileName);
        }

        public void Delete()
        {
            OnDeleting();
            _fakeFileSystem.Remove(_file.FullName);
            FakeFileWatcher.HandleFileChanged(_file.FullName);
        }

        public static MemoryStream ExpandableMemoryStream(byte[] buffer)
        {
            MemoryStream stream = new MemoryStream(buffer.Length);
            stream.Write(buffer, 0, buffer.Length);
            stream.Position = 0;

            return stream;
        }

        public void CreateFolder()
        {
            string directory = Path.GetDirectoryName(_file.FullName);
            DateTime utcNow = DateTime.UtcNow;

            if (_fakeFileSystem.ContainsKey(directory))
            {
                return;
            }
            AddFile(directory, true, utcNow, utcNow, utcNow, Stream.Null);
        }

        public void CreateNewFile()
        {
            OnExceptionHook("CreateNewFile");

            FakeFileInfo fileInfo = FindFileInfo();
            if (fileInfo != null)
            {
                throw new InternalErrorException("File exists.", ErrorStatus.FileExists);
            }
            AddFile(FullName, new MemoryStream());
        }

        public IEnumerable<IDataStore> Files
        {
            get
            {
                if (!IsAvailable)
                {
                    return new IDataStore[0];
                }

                List<FakeFileInfo> files = new List<FakeFileInfo>();
                foreach (KeyValuePair<string, FakeFileInfo> kvp in _fakeFileSystem)
                {
                    if (kvp.Value.IsFolder)
                    {
                        continue;
                    }
                    if (!kvp.Value.FullName.StartsWith(FullName, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    files.Add(kvp.Value);
                }
                return files.Select((FakeFileInfo fileInfo) => { return New<IDataStore>(fileInfo.FullName); });
            }
        }

        /// <summary>
        /// Removes a folder in the underlying file system with the path of this instance,
        /// if the folder is empty. If it is not, nothing happens.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void RemoveFolder()
        {
            if (!IsAvailable)
            {
                return;
            }
            RemoveFileOrFolder(FullName);
        }

        public IDataContainer Container
        {
            get { return new FakeDataContainer(Resolve.Portable.Path().GetDirectoryName(_file.FullName)); }
        }

        public virtual bool IsFile
        {
            get { return !IsFolder; }
        }

        public virtual bool IsFolder
        {
            get
            {
                FakeFileInfo ffi = FindFileInfo();
                if (ffi == null)
                {
                    return true;
                }
                return ffi.IsFolder;
            }
        }
    }
}
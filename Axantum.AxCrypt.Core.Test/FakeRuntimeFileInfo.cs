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
using System.IO;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.IO;

namespace Axantum.AxCrypt.Core.Test
{
    internal class FakeRuntimeFileInfo : IRuntimeFileInfo
    {
        private class FakeFileInfo
        {
            public DateTime CreationTimeUtc;
            public DateTime LastAccessTimeUtc;
            public DateTime LastWriteTimeUtc;
            public Stream Stream;
        }

        private static Dictionary<string, FakeFileInfo> _fakeFileSystem = new Dictionary<string, FakeFileInfo>();

        public static void AddFile(string path, DateTime creationTimeUtc, DateTime lastAccessTimeUtc, DateTime lastWriteTimeUtc, Stream stream)
        {
            _fakeFileSystem.Add(path, new FakeFileInfo { CreationTimeUtc = creationTimeUtc, LastAccessTimeUtc = lastAccessTimeUtc, LastWriteTimeUtc = lastWriteTimeUtc, Stream = stream });
        }

        public static void AddFile(string path, DateTime timeUtc, Stream stream)
        {
            AddFile(path, timeUtc, timeUtc, timeUtc, stream);
        }

        public static void ClearFiles()
        {
            _fakeFileSystem.Clear();
        }

        private FileInfo _file;

        public FakeRuntimeFileInfo(FileInfo file)
        {
            _file = new FileInfo(file.FullName);
        }

        private FakeFileInfo FindFileInfo()
        {
            FakeFileInfo fakeFileInfo;
            if (!_fakeFileSystem.TryGetValue(_file.FullName, out fakeFileInfo))
            {
                throw new DirectoryNotFoundException(_file.FullName);
            }
            return fakeFileInfo;
        }

        public Stream OpenRead()
        {
            FakeFileInfo fakeFileInfo = FindFileInfo();
            return new NonClosingStream(fakeFileInfo.Stream);
        }

        public Stream OpenWrite()
        {
            FakeFileInfo fakeFileInfo = FindFileInfo();
            return new NonClosingStream(fakeFileInfo.Stream);
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
                FakeFileInfo fakeFileInfo = FindFileInfo();
                return fakeFileInfo.CreationTimeUtc;
            }
        }

        public DateTime LastAccessTimeUtc
        {
            get
            {
                FakeFileInfo fakeFileInfo = FindFileInfo();
                return fakeFileInfo.LastAccessTimeUtc;
            }
        }

        public DateTime LastWriteTimeUtc
        {
            get
            {
                FakeFileInfo fakeFileInfo = FindFileInfo();
                return fakeFileInfo.LastWriteTimeUtc;
            }
        }
    }
}
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

using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Test
{
    internal class FakeRuntimeFolderInfo : IRuntimeFolderInfo
    {
        private FakeRuntimeFileInfo _fileInfo;

        public FakeRuntimeFolderInfo(string path)
        {
            _fileInfo = new FakeRuntimeFileInfo(path);
        }

        /// <summary>
        /// Combine the path of this instance with another path, creating a new instance.
        /// </summary>
        /// <param name="path">The path to combine with.</param>
        /// <returns>
        /// A new instance representing the combined path.
        /// </returns>
        public IRuntimeFileInfo FileItemInfo(string path)
        {
            path = path.NormalizeFilePath();
            return new FakeRuntimeFileInfo(Path.Combine(FullName, path));
        }

        /// <summary>
        /// Combine the path of this instance with another path, creating a new instance.
        /// </summary>
        /// <param name="path">The path to combine with.</param>
        /// <returns>
        /// A new instance representing the combined path.
        /// </returns>
        public IRuntimeFolderInfo FolderItemInfo(string path)
        {
            path = path.NormalizeFilePath();
            return new FakeRuntimeFolderInfo(Path.Combine(FullName, path));
        }

        public bool IsFolder
        {
            get
            {
                return true;
            }
        }

        public bool IsFile
        {
            get
            {
                return false;
            }
        }

        public void CreateFolder(string item)
        {
            FakeRuntimeFileInfo.AddFolder(Resolve.Portable.Path().Combine(_fileInfo.FullName, item));
        }

        public void RemoveFolder(string item)
        {
            FakeRuntimeFileInfo.RemoveFileOrFolder(Resolve.Portable.Path().Combine(_fileInfo.FullName, item));
        }

        public IRuntimeFileInfo CreateNewFile(string item)
        {
            FakeRuntimeFileInfo frfi = new FakeRuntimeFileInfo(Resolve.Portable.Path().Combine(_fileInfo.FullName, item));
            frfi.CreateNewFile();
            return frfi;
        }

        public void CreateFolder()
        {
            _fileInfo.CreateFolder();
        }

        public IEnumerable<IRuntimeFileInfo> Files
        {
            get { return _fileInfo.Files; }
        }

        public bool IsAvailable
        {
            get { return _fileInfo.IsAvailable; }
        }

        public string Name
        {
            get { return _fileInfo.Name; }
        }

        public string FullName
        {
            get { return _fileInfo.FullName; }
        }

        public void Delete()
        {
            _fileInfo.Delete();
        }
    }
}
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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Fake
{
    public class FakeDataContainer : IDataContainer
    {
        private FakeDataStore _fileInfo;

        public FakeDataContainer(string path)
        {
            _fileInfo = new FakeDataStore(path.NormalizeFolderPath());
        }

        /// <summary>
        /// Combine the path of this instance with another path, creating a new instance.
        /// </summary>
        /// <param name="path">The path to combine with.</param>
        /// <returns>
        /// A new instance representing the combined path.
        /// </returns>
        public IDataStore FileItemInfo(string path)
        {
            path = path.NormalizeFilePath();
            return new FakeDataStore(Path.Combine(FullName, path));
        }

        /// <summary>
        /// Combine the path of this instance with another path, creating a new instance.
        /// </summary>
        /// <param name="path">The path to combine with.</param>
        /// <returns>
        /// A new instance representing the combined path.
        /// </returns>
        public IDataContainer FolderItemInfo(string path)
        {
            path = path.NormalizeFilePath();
            return new FakeDataContainer(Path.Combine(FullName, path));
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
            FakeDataStore.AddFolder(Resolve.Portable.Path().Combine(_fileInfo.FullName, item));
        }

        public void RemoveFolder(string item)
        {
            FakeDataStore.RemoveFileOrFolder(Resolve.Portable.Path().Combine(_fileInfo.FullName, item));
        }

        public IDataStore CreateNewFile(string item)
        {
            FakeDataStore frfi = new FakeDataStore(Resolve.Portable.Path().Combine(_fileInfo.FullName, item));
            frfi.CreateNewFile();
            return frfi;
        }

        public void CreateFolder()
        {
            _fileInfo.CreateFolder();
        }

        public IEnumerable<IDataStore> Files
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

        public IDataContainer Container
        {
            get { return new FakeDataContainer(Resolve.Portable.Path().GetDirectoryName(_fileInfo.FullName)); }
        }

        public IEnumerable<IDataContainer> Folders
        {
            get
            {
                return new IDataContainer[0];
            }
        }
    }
}
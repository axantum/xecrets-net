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

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Mono
{
    public class DataContainer : DataItem, IDataContainer
    {
        private DirectoryInfo _info;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataStore"/> class.
        /// </summary>
        /// <param name="fullName">The full path and name of the file or folder.</param>
        /// <exception cref="System.ArgumentNullException">fullName</exception>
        public DataContainer(string fullName)
        {
            if (fullName == null)
            {
                throw new ArgumentNullException("fullName");
            }

            _info = new DirectoryInfo(fullName.NormalizeFolderPath());
        }

        /// <summary>
        /// Combine the path of this instance with another path, creating a new instance.
        /// </summary>
        /// <param name="path">The path to combine with.</param>
        /// <returns>
        /// A new instance representing the combined path.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IDataStore FileItemInfo(string item)
        {
            return new DataStore(Path.Combine(FullName, item));
        }

        /// <summary>
        /// Get a folder item from this instance (which must represent a folder or container)..
        /// </summary>
        /// <param name="item">The name of the file item.</param>
        /// <returns>
        /// A new instance representing the file item in the folder or container.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IDataContainer FolderItemInfo(string item)
        {
            return new DataContainer(Path.Combine(FullName, item));
        }

        /// <summary>
        /// Creates a folder in the underlying file system with the path of this instance.
        /// </summary>
        public void CreateFolder(string item)
        {
            _info.Refresh();
            Directory.CreateDirectory(Path.Combine(_info.FullName, item));
        }

        public void CreateFolder()
        {
            _info.Refresh();
            _info.Create();
        }

        /// <summary>
        /// Removes a folder in the underlying file system with the path of this instance,
        /// if the folder is empty. If it is not, nothing happens.
        /// </summary>
        public void RemoveFolder(string item)
        {
            IDataContainer folder = FolderItemInfo(item);
            if (!folder.IsAvailable)
            {
                return;
            }
            DirectoryInfo di = new DirectoryInfo(folder.FullName);
            if (di.EnumerateFiles().Any() || di.EnumerateDirectories().Any())
            {
                return;
            }
            di.Delete();
        }

        /// <summary>
        /// Creates a file in the underlying system. If it already exists, an AxCryptException is thrown with status FileExists.
        /// </summary>
        public IDataStore CreateNewFile(string item)
        {
            _info.Refresh();
            FileInfo file = new FileInfo(Path.Combine(_info.FullName, item));
            try
            {
                using (FileStream stream = new FileStream(file.FullName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
                {
                    return new DataStore(file.FullName);
                }
            }
            catch (IOException ioex)
            {
                throw new InternalErrorException("File exists.", ErrorStatus.FileExists, ioex);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is a folder that exists.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is folder that exists; otherwise, <c>false</c>.
        /// </value>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool IsAvailable
        {
            get
            {
                _info.Refresh();
                return _info.Exists;
            }
        }

        /// <summary>
        /// Enumerate all files (not folders) in this folder, if it's a folder.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<IDataStore> Files
        {
            get
            {
                if (!IsAvailable)
                {
                    return new IDataStore[0];
                }
                _info.Refresh();
                return _info.EnumerateFiles().Select((fi) => new DataStore(fi.FullName));
            }
        }

        public IEnumerable<IDataContainer> Folders
        {
            get
            {
                if (!IsAvailable)
                {
                    return new IDataContainer[0];
                }
                _info.Refresh();
                return _info.EnumerateDirectories().Select((fi) => new DataContainer(fi.FullName));
            }
        }

        public override bool IsFile
        {
            get { return false; }
        }

        public override bool IsFolder
        {
            get { return true; }
        }

        public override string Name
        {
            get
            {
                _info.Refresh();
                return _info.Name;
            }
        }

        public override void Delete()
        {
            _info.Refresh();
            if (_info.EnumerateDirectories().Any() || _info.EnumerateFiles().Any())
            {
                return;
            }
            _info.Delete();
        }

        public override string FullName
        {
            get
            {
                _info.Refresh();
                return _info.FullName;
            }
        }
    }
}
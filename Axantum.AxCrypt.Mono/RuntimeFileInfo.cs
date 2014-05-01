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

using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Axantum.AxCrypt.Mono
{
    /// <summary>
    /// Provides properties and instance methods for the operations with files, and aids in the creation of Stream objects. The underlying file must not
    /// necessarily exist.
    /// </summary>
    public class RuntimeFileInfo : IRuntimeFileInfo
    {
        private FileInfo _file;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeFileInfo"/> class.
        /// </summary>
        /// <param name="fullName">The full path and name of the file or folder.</param>
        /// <exception cref="System.ArgumentNullException">fullName</exception>
        public RuntimeFileInfo(string fullName)
        {
            if (fullName == null)
            {
                throw new ArgumentNullException("fullName");
            }
            _file = new FileInfo(fullName.NormalizeFilePath());
        }

        private RuntimeFileInfo(FileInfo fileInfo)
        {
            _file = fileInfo;
        }

        /// <summary>
        /// Opens a stream in read mode for the underlying file.
        /// </summary>
        /// <returns>
        /// A stream opened for reading.
        /// </returns>
        public Stream OpenRead()
        {
            Stream stream = new FileStream(_file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, OS.Current.StreamBufferSize);
            return new LockingStream(this, stream);
        }

        /// <summary>
        /// Opens a stream in write mode for the underlying file.
        /// </summary>
        /// <returns>
        /// A stream opened for writing.
        /// </returns>
        public Stream OpenWrite()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_file.FullName));
            Stream stream = new FileStream(_file.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, OS.Current.StreamBufferSize);
            return new LockingStream(this, stream);
        }

        /// <summary>
        /// Get the Name part without the folder part of the path.
        /// </summary>
        public string Name
        {
            get
            {
                return _file.Name;
            }
        }

        /// <summary>
        /// Gets or sets the creation time UTC.
        /// </summary>
        /// <value>
        /// The creation time UTC.
        /// </value>
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

        /// <summary>
        /// Gets or sets the last access time UTC.
        /// </summary>
        /// <value>
        /// The last access time UTC.
        /// </value>
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

        /// <summary>
        /// Gets or sets the last write time UTC.
        /// </summary>
        /// <value>
        /// The last write time UTC.
        /// </value>
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

        /// <summary>
        /// Sets all of the file times of the underlying file.
        /// </summary>
        /// <param name="creationTimeUtc">The creation time UTC.</param>
        /// <param name="lastAccessTimeUtc">The last access time UTC.</param>
        /// <param name="lastWriteTimeUtc">The last write time UTC.</param>
        public void SetFileTimes(DateTime creationTimeUtc, DateTime lastAccessTimeUtc, DateTime lastWriteTimeUtc)
        {
            CreationTimeUtc = creationTimeUtc;
            LastAccessTimeUtc = lastAccessTimeUtc;
            LastWriteTimeUtc = lastWriteTimeUtc;
        }

        /// <summary>
        /// Get the full name including drive, directory and file name if any
        /// </summary>
        public string FullName
        {
            get { return _file.FullName; }
        }

        /// <summary>
        /// Gets a value indicating whether the file this <see cref="IRuntimeFileInfo" /> represents exists in the underlying file system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if exists; otherwise, <c>false</c>.
        /// </value>
        public bool IsExistingFile
        {
            get
            {
                _file.Refresh();
                return _file.Exists;
            }
        }

        /// <summary>
        /// Moves the underlying file to a new location.
        /// </summary>
        /// <param name="destinationFileName">Name of the destination file.</param>
        public void MoveTo(string destinationFileName)
        {
            IRuntimeFileInfo destination = Factory.New<IRuntimeFileInfo>(destinationFileName);
            if (destination.IsExistingFile)
            {
                try
                {
                    File.Replace(_file.FullName, destinationFileName, null);
                    _file = new FileInfo(destinationFileName);
                    return;
                }
                catch (PlatformNotSupportedException)
                {
                }
                destination.Delete();
            }
            _file.MoveTo(destinationFileName);
        }

        /// <summary>
        /// Deletes the underlying file this instance refers to.
        /// </summary>
        public void Delete()
        {
            _file.Delete();
        }

        /// <summary>
        /// Creates a folder in the underlying file system with the path of this instance.
        /// </summary>
        public void CreateFolder()
        {
            Directory.CreateDirectory(_file.FullName);
        }

        /// <summary>
        /// Creates a file in the underlying system. If it already exists, an AxCryptException is thrown with status FileExists.
        /// </summary>
        public void CreateNewFile()
        {
            try
            {
                using (FileStream stream = new FileStream(_file.FullName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
                {
                    return;
                }
            }
            catch (IOException)
            {
                throw new InternalErrorException("File exists.", ErrorStatus.FileExists);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is a folder that exists.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is folder that exists; otherwise, <c>false</c>.
        /// </value>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool IsExistingFolder
        {
            get
            {
                return (int)_file.Attributes != -1 && (_file.Attributes & FileAttributes.Directory) == FileAttributes.Directory;
            }
        }

        /// <summary>
        /// Enumerate all files (not folders) in this folder, if it's a folder.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<IRuntimeFileInfo> Files
        {
            get
            {
                if (!IsExistingFolder)
                {
                    return new IRuntimeFileInfo[0];
                }
                DirectoryInfo di = new DirectoryInfo(_file.FullName);
                return di.GetFiles().Select((FileInfo fi) => { return (IRuntimeFileInfo)new RuntimeFileInfo(fi); });
            }
        }

        /// <summary>
        /// Combine the path of this instance with another path, creating a new instance.
        /// </summary>
        /// <param name="path">The path to combine with.</param>
        /// <returns>
        /// A new instance representing the combined path.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IRuntimeFileInfo Combine(string path)
        {
            return new RuntimeFileInfo(Path.Combine(FullName, path));
        }
    }
}
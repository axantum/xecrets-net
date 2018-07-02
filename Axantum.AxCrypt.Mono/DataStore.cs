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
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Mono
{
    /// <summary>
    /// Provides properties and instance methods for the operations with files, and aids in the creation of Stream objects. The underlying file must not
    /// necessarily exist.
    /// </summary>
    public class DataStore : DataItem, IDataStore
    {
        private FileInfo _file;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataStore"/> class.
        /// </summary>
        /// <param name="path">The full path and name of the file or folder.</param>
        /// <exception cref="System.ArgumentNullException">fullName</exception>
        public DataStore(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            string normalized = path.NormalizeFilePath();
            try
            {
                _file = new FileInfo(normalized);
            }
            catch (ArgumentException ae)
            {
                throw new FileOperationException($"Can't create {nameof(DataStore)}.", normalized, ErrorStatus.Exception, ae);
            }
        }

        protected override string Location
        {
            get
            {
                _file.Refresh();
                return _file.FullName;
            }
            set
            {
                throw new NotSupportedException("The location cannot be set explicitly.");
            }
        }

        /// <summary>
        /// Opens a stream in read mode for the underlying file.
        /// </summary>
        /// <returns>
        /// A stream opened for reading.
        /// </returns>
        public Stream OpenRead()
        {
            _file.Refresh();
            Stream stream = new FileStream(_file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, OS.Current.StreamBufferSize);
            return stream;
        }

        /// <summary>
        /// Opens a stream in write mode for the underlying file.
        /// </summary>
        /// <returns>
        /// A stream opened for writing.
        /// </returns>
        public Stream OpenWrite()
        {
            Stream stream = OpenUpdate();
            stream.SetLength(0);
            return stream;
        }

        /// <summary>
        /// Opens a stream in update mode for the underlying file.
        /// </summary>
        /// <returns>
        /// A stream opened for updating, keeping existing data if any.
        /// </returns>
        public Stream OpenUpdate()
        {
            _file.Refresh();
            Directory.CreateDirectory(Path.GetDirectoryName(_file.FullName));
            _file.Refresh();
            FileStream stream = new FileStream(_file.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, OS.Current.StreamBufferSize);
            return stream;
        }

        public bool IsWriteProtected
        {
            get
            {
                _file.Refresh();
                return _file.IsReadOnly;
            }
            set
            {
                try
                {
                    _file.Refresh();
                    _file.IsReadOnly = value;
                }
                catch (Exception ex)
                {
                    _file.Refresh();
                    throw new FileOperationException("Could not change write protection.", _file.FullName, ErrorStatus.Exception, ex);
                }
            }
        }

        public bool IsLocked()
        {
            try
            {
                _file.Refresh();
                using (Stream stream = _file.Open(FileMode.Open, IsWriteProtected ? FileAccess.Read : FileAccess.ReadWrite, IsWriteProtected ? FileShare.Read : FileShare.None))
                {
                    return false;
                }
            }
            catch (IOException ioex)
            {
                New<IReport>().Exception(ioex);
                return true;
            }
            catch (Exception ex)
            {
                _file.Refresh();
                throw new FileOperationException("Lock failed", _file.FullName, ErrorStatus.Exception, ex);
            }
        }

        public bool IsEncryptable()
        {
            FileAttributes attributes = File.GetAttributes(_file.FullName);

            if ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get the Name part without the folder part of the path.
        /// </summary>
        public override string Name
        {
            get
            {
                _file.Refresh();
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
                return FileTimeSafe(() => _file.CreationTimeUtc);
            }
            set
            {
                _file.Refresh();
                _file.CreationTimeUtc = Validate(value);
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
                return FileTimeSafe(() => _file.LastAccessTimeUtc);
            }
            set
            {
                _file.Refresh();
                _file.LastAccessTimeUtc = Validate(value);
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
                return FileTimeSafe(() => _file.LastWriteTimeUtc);
            }
            set
            {
                _file.Refresh();
                _file.LastWriteTimeUtc = Validate(value);
            }
        }

        private static readonly DateTime MIN_FILETIME = new DateTime(1900, 1, 1);

        private static readonly DateTime MAX_FILETIME = new DateTime(2200, 1, 1);

        private static DateTime FileTimeSafe(Func<DateTime> fileTimeFunc)
        {
            try
            {
                return fileTimeFunc();
            }
            catch (ArgumentOutOfRangeException)
            {
                return New<INow>().Utc;
            }
        }

        private static DateTime Validate(DateTime fileTime)
        {
            if (fileTime < MIN_FILETIME)
            {
                return MIN_FILETIME;
            }
            if (fileTime > MAX_FILETIME)
            {
                return MAX_FILETIME;
            }
            return fileTime;
        }

        /// <summary>
        /// Sets all of the file times of the underlying file.
        /// </summary>
        /// <param name="creationTimeUtc">The creation time UTC.</param>
        /// <param name="lastAccessTimeUtc">The last access time UTC.</param>
        /// <param name="lastWriteTimeUtc">The last write time UTC.</param>
        public void SetFileTimes(DateTime creationTimeUtc, DateTime lastAccessTimeUtc, DateTime lastWriteTimeUtc)
        {
            DateTime utcNow = New<INow>().Utc;
            CreationTimeUtc = creationTimeUtc <= utcNow ? creationTimeUtc : utcNow;
            LastAccessTimeUtc = lastAccessTimeUtc <= utcNow ? lastAccessTimeUtc : utcNow;
            LastWriteTimeUtc = lastWriteTimeUtc <= utcNow ? lastWriteTimeUtc : utcNow;
        }

        /// <summary>
        /// Get the full name including drive, directory and file name if any
        /// </summary>
        public override string FullName
        {
            get
            {
                _file.Refresh();
                return _file.FullName;
            }
        }

        /// <summary>
        /// Moves the underlying file to a new location.
        /// </summary>
        /// <param name="destinationFileName">Name of the destination file.</param>
        public void MoveTo(string destinationFileName)
        {
            IDataStore destination = New<IDataStore>(destinationFileName);
            if (destination.IsAvailable)
            {
                destination.Delete();
            }
            _file.Refresh();
            _file.MoveTo(destinationFileName);
        }

        /// <summary>
        /// Deletes the underlying file this instance refers to.
        /// </summary>
        public override void Delete()
        {
            _file.Refresh();
            _file.Delete();
        }

        public long Length()
        {
            return _file.Length;
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
                _file.Refresh();
                return _file.Exists;
            }
        }

        public override bool IsFile
        {
            get { return true; }
        }

        public override bool IsFolder
        {
            get { return false; }
        }
    }
}
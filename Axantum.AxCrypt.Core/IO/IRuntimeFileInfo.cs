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

namespace Axantum.AxCrypt.Core.IO
{
    /// <summary>
    /// Abstraction for FileInfo-related operations. Provides properties and instance methods for the operations with files, and aids in the creation of Stream objects.
    /// </summary>
    public interface IRuntimeFileInfo
    {
        /// <summary>
        /// Opens a stream in read mode for the underlying file.
        /// </summary>
        /// <returns>A stream opened for reading.</returns>
        Stream OpenRead();

        /// <summary>
        /// Opens a stream in write mode for the underlying file.
        /// </summary>
        /// <returns>A stream opened for writing.</returns>
        Stream OpenWrite();

        /// <summary>
        /// Creates a folder in the underlying file system with the path of this instance.
        /// </summary>
        void CreateFolder();

        /// <summary>
        /// Creates a file in the underlying system. If it already exists, an AxCryptException is thrown with status FileExists.
        /// </summary>
        void CreateNewFile();

        /// <summary>
        /// Get the Name part without the folder part of the path.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Get the full name including drive, directory and file name if any
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Gets or sets the creation time UTC.
        /// </summary>
        /// <value>
        /// The creation time UTC.
        /// </value>
        DateTime CreationTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the last access time UTC.
        /// </summary>
        /// <value>
        /// The last access time UTC.
        /// </value>
        DateTime LastAccessTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the last write time UTC.
        /// </summary>
        /// <value>
        /// The last write time UTC.
        /// </value>
        DateTime LastWriteTimeUtc { get; set; }

        /// <summary>
        /// Sets all of the file times of the underlying file.
        /// </summary>
        /// <param name="creationTimeUtc">The creation time UTC.</param>
        /// <param name="lastAccessTimeUtc">The last access time UTC.</param>
        /// <param name="lastWriteTimeUtc">The last write time UTC.</param>
        void SetFileTimes(DateTime creationTimeUtc, DateTime lastAccessTimeUtc, DateTime lastWriteTimeUtc);

        /// <summary>
        /// Gets a value indicating whether the file this <see cref="IRuntimeFileInfo"/> represents exists in the underlying file system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if exists; otherwise, <c>false</c>.
        /// </value>
        bool Exists { get; }

        /// <summary>
        /// Moves the underlying file to a new location.
        /// </summary>
        /// <param name="destinationFileName">Name of the destination file.</param>
        void MoveTo(string destinationFileName);

        /// <summary>
        /// Deletes the underlying file this instance refers to.
        /// </summary>
        void Delete();

        /// <summary>
        /// Gets a value indicating whether this instance is folder.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is folder; otherwise, <c>false</c>.
        /// </value>
        bool IsFolder { get; }

        /// <summary>
        /// Enumerate all files (not folders) in this folder, if it's a folder.
        /// </summary>
        IEnumerable<IRuntimeFileInfo> Files { get; }

        /// <summary>
        /// Combine the path of this instance with another path, creating a new instance.
        /// </summary>
        /// <param name="path">The path to combine with.</param>
        /// <returns>A new instance representing the combined path.</returns>
        IRuntimeFileInfo Combine(string path);
    }
}
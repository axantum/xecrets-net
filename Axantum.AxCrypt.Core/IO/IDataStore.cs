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

using System;
using System.Collections.Generic;
using System.IO;

namespace Axantum.AxCrypt.Core.IO
{
    /// <summary>
    /// Abstraction for FileInfo-related operations. Provides properties and instance methods for the operations with files, and aids in the creation of Stream objects.
    /// </summary>
    public interface IDataStore : IDataItem
    {
        /// <summary>
        /// Opens a stream in read mode for the underlying file.
        /// </summary>
        /// <returns>A stream opened for reading.</returns>
        Stream OpenRead();

        /// <summary>
        /// Opens a stream in write mode for the underlying file.
        /// </summary>
        /// <returns>A stream opened for writing, always truncatd to zero length.</returns>
        Stream OpenWrite();

        /// <summary>
        /// Opens a stream in update mode for the underlying file.
        /// </summary>
        /// <returns>A stream opened for updating, keeping existing data if any.</returns>
        Stream OpenUpdate();

        /// <summary>
        /// Determine if the file is currently unavailable for exclusive locking.
        /// </summary>
        /// <returns></returns>
        bool IsLocked { get; }

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
        /// Moves the underlying file to a new location.
        /// </summary>
        /// <param name="destinationFileName">Name of the destination file.</param>
        void MoveTo(string destinationFileName);
    }
}
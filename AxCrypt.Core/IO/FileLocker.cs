#region Coypright and License

/*
 * AxCrypt - Copyright 2017, Svante Seleborg, All Rights Reserved
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
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Core.IO
{
    public sealed class FileLocker
    {
        private readonly Dictionary<string, FileLockManager> _lockedFiles = new Dictionary<string, FileLockManager>();

        public FileLock Acquire(IDataItem dataItem)
        {
#if DEBUG
            return Acquire(dataItem, TimeSpan.FromSeconds(10));
#else
            return Acquire(dataItem, TimeSpan.FromMilliseconds(-1));
#endif
        }

        public FileLock Acquire(IDataItem dataItem, TimeSpan timeout)
        {
            if (dataItem == null)
            {
                throw new ArgumentNullException(nameof(dataItem));
            }

            FileLockManager fileLockManager;
            lock (_lockedFiles)
            {
                fileLockManager = GetOrCreateFileLockUnsafe(dataItem.FullName, timeout);
                _ = fileLockManager.IncrementReferenceCount();
            }
            FileLock fileLock = fileLockManager.GetFileLock();
            _ = fileLockManager.DecrementReferenceCount();
            return fileLock;
        }

        public async Task<FileLock> AcquireAsync(IDataItem dataItem, TimeSpan timeout)
        {
            if (dataItem == null)
            {
                throw new ArgumentNullException(nameof(dataItem));
            }

            FileLockManager fileLockManager;
            lock (_lockedFiles)
            {
                fileLockManager = GetOrCreateFileLockUnsafe(dataItem.FullName, timeout);
                _ = fileLockManager.IncrementReferenceCount();
            }
            FileLock fileLock = await fileLockManager.GetFileLockAsync();
            _ = fileLockManager.DecrementReferenceCount();
            return fileLock;
        }

        private FileLockManager GetOrCreateFileLockUnsafe(string fullName, TimeSpan timeout)
        {
            if (!_lockedFiles.TryGetValue(fullName, out FileLockManager? fileLockManager))
            {
                fileLockManager = new FileLockManager(fullName, timeout, this);
                _lockedFiles[fullName] = fileLockManager;
            }
            return fileLockManager;
        }

        public bool IsLocked(params IDataStore[] dataStoreParameters)
        {
            if (dataStoreParameters == null)
            {
                throw new ArgumentNullException(nameof(dataStoreParameters));
            }

            foreach (IDataStore dataStore in dataStoreParameters)
            {
                if (dataStore == null)
                {
                    throw new ArgumentNullException(nameof(dataStoreParameters));
                }

                if (IsLocked(dataStore.FullName))
                {
                    if (Resolve.Log.IsInfoEnabled)
                    {
                        Resolve.Log.LogInfo("File '{0}' was found to be locked.".InvariantFormat(dataStore.FullName));
                    }
                    return true;
                }
            }
            return false;
        }

        private bool IsLocked(string fullName)
        {
            lock (_lockedFiles)
            {
                if (!_lockedFiles.TryGetValue(fullName, out FileLockManager? fileLockManager))
                {
                    return false;
                }

                return fileLockManager.IsLocked;
            }
        }

        internal bool TryRemove(string fullName)
        {
            lock (_lockedFiles)
            {
                if (!_lockedFiles.TryGetValue(fullName, out FileLockManager? fileLockManager))
                {
                    return true;
                }

                if (fileLockManager.DecrementReferenceCount() == 0)
                {
                    fileLockManager.Dispose();
                    _ = _lockedFiles.Remove(fullName);
                    return true;
                }

                return false;
            }
        }
    }
}

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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.IO
{
    public sealed class FileLocker
    {
        private Dictionary<string, FileLockManager> _lockedFiles = new Dictionary<string, FileLockManager>();

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
                throw new ArgumentNullException("dataItem");
            }

            lock (_lockedFiles)
            {
                FileLockManager fileLock = GetOrCreateFileLockUnsafe(dataItem.FullName, timeout);
                return fileLock.Lock();
            }
        }

        public Task<FileLock> AcquireAsync(IDataItem dataItem, TimeSpan timeout)
        {
            if (dataItem == null)
            {
                throw new ArgumentNullException("dataItem");
            }

            lock (_lockedFiles)
            {
                FileLockManager fileLockManager = GetOrCreateFileLockUnsafe(dataItem.FullName, timeout);
                return fileLockManager.LockAsync();
            }
        }

        private FileLockManager GetOrCreateFileLockUnsafe(string fullName, TimeSpan timeout)
        {
            FileLockManager fileLock = null;
            if (!_lockedFiles.TryGetValue(fullName, out fileLock))
            {
                fileLock = new FileLockManager(fullName, timeout, this);
                _lockedFiles[fullName] = fileLock;
            }
            return fileLock;
        }

        public bool IsLocked(params IDataStore[] dataStoreParameters)
        {
            if (dataStoreParameters == null)
            {
                throw new ArgumentNullException("dataStoreParameters");
            }

            foreach (IDataStore dataStore in dataStoreParameters)
            {
                if (dataStore == null)
                {
                    throw new ArgumentNullException("dataStoreParameters");
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
                FileLockManager fileLock;
                if (!_lockedFiles.TryGetValue(fullName, out fileLock))
                {
                    return false;
                }

                return fileLock.CurrentCount == 0;
            }
        }

        public void Release(string fullName)
        {
            lock (_lockedFiles)
            {
                _lockedFiles.Remove(fullName);
            }
        }
    }
}
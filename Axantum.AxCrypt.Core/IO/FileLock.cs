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

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Axantum.AxCrypt.Core.IO
{
    public class FileLock : IDisposable
    {
        private static Dictionary<string, FileLock> _lockedFiles = new Dictionary<string, FileLock>();

        private object _lock = new object();

        private int _referenceCount = 0;

        private string _originalLockedFileName;

        private FileLock(string fullName)
        {
            _originalLockedFileName = fullName;
        }

        public IDataStore DataStore { get { return TypeMap.Resolve.New<IDataStore>(_originalLockedFileName); } }

        public static FileLock Lock(IDataItem dataItem)
        {
            if (dataItem == null)
            {
                throw new ArgumentNullException("dataItem");
            }

            while (true)
            {
                lock (_lockedFiles)
                {
                    FileLock fileLock = GetOrCreateFileLock(dataItem.FullName);
                    bool lockTaken = false;
                    try
                    {
                        Monitor.TryEnter(fileLock._lock, ref lockTaken);
                        if (!lockTaken)
                        {
                            continue;
                        }
                        ++fileLock._referenceCount;
                        if (Resolve.Log.IsInfoEnabled)
                        {
                            Resolve.Log.LogInfo("Locking file '{0}'.".InvariantFormat(dataItem.FullName));
                        }
                        return fileLock;
                    }
                    catch
                    {
                        if (lockTaken)
                        {
                            fileLock.Dispose();
                        }
                        throw;
                    }
                }
            }
        }

        private static FileLock GetOrCreateFileLock(string fullName)
        {
            FileLock fileLock = null;
            if (!_lockedFiles.TryGetValue(fullName, out fileLock))
            {
                fileLock = new FileLock(fullName);
                _lockedFiles[fullName] = fileLock;
            }
            return fileLock;
        }

        public static bool IsLocked(params IDataStore[] dataStoreParameters)
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

                if (TestLocked(dataStore.FullName))
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

        private static bool TestLocked(string fullName)
        {
            lock (_lockedFiles)
            {
                if (!_lockedFiles.Keys.Contains(fullName))
                {
                    return false;
                }

                bool lockTaken = false;
                try
                {
                    Monitor.TryEnter(_lockedFiles[fullName]._lock, ref lockTaken);
                }
                finally
                {
                    if (lockTaken)
                    {
                        Monitor.Exit(_lockedFiles[fullName]._lock);
                    }
                }
                return !lockTaken;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            lock (_lockedFiles)
            {
                if (_referenceCount == 0)
                {
                    return;
                }

                Monitor.Exit(_lock);
                if (--_referenceCount == 0)
                {
                    _lockedFiles.Remove(_originalLockedFileName);
                }

                if (Resolve.Log.IsInfoEnabled)
                {
                    Resolve.Log.LogInfo("Unlocking file '{0}'.".InvariantFormat(DataStore.FullName));
                }
            }
        }
    }
}
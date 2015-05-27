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
using System;
using System.Collections.ObjectModel;

namespace Axantum.AxCrypt.Core.IO
{
    public class FileLock : IDisposable
    {
        private static Collection<string> _lockedFiles = new Collection<string>();

        private FileLock(IDataStore dataStore)
        {
            DataStore = dataStore;
            _originalLockedFileName = dataStore.FullName;
        }

        private string _originalLockedFileName;

        public IDataStore DataStore { get; private set; }

        public static FileLock Lock(IDataStore dataStore)
        {
            if (dataStore == null)
            {
                throw new ArgumentNullException("dataStore");
            }

            while (true)
            {
                lock (_lockedFiles)
                {
                    if (IsLocked(dataStore))
                    {
                        continue;
                    }

                    _lockedFiles.Add(dataStore.FullName);
                    if (Resolve.Log.IsInfoEnabled)
                    {
                        Resolve.Log.LogInfo("Locking file '{0}'.".InvariantFormat(dataStore.FullName));
                    }
                    return new FileLock(dataStore);
                }
            }
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
                lock (_lockedFiles)
                {
                    if (_lockedFiles.Contains(dataStore.FullName))
                    {
                        if (Resolve.Log.IsInfoEnabled)
                        {
                            Resolve.Log.LogInfo("File '{0}' was found to be locked.".InvariantFormat(dataStore.FullName));
                        }
                        return true;
                    }
                }
            }
            return false;
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
                if (DataStore == null)
                {
                    return;
                }
                _lockedFiles.Remove(_originalLockedFileName);
                if (Resolve.Log.IsInfoEnabled)
                {
                    Resolve.Log.LogInfo("Unlocking file '{0}'.".InvariantFormat(DataStore.FullName));
                }
                DataStore = null;
                _originalLockedFileName = null;
            }
        }
    }
}
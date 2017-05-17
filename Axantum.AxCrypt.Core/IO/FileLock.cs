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
using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.IO
{
    public sealed class FileLockReleaser : IDisposable
    {
        private readonly FileLock m_toRelease;

        private FileLockReleaser(FileLock toRelease)
        {
            m_toRelease = toRelease;
        }

        public static FileLockReleaser Acquire(IDataItem dataItem)
        {
            return FileLock.Lock(dataItem);
        }

        public IDataStore DataStore { get { return m_toRelease.DataStore; } }

        public static bool IsLocked(params IDataStore[] dataItems)
        {
            return FileLock.IsLocked(dataItems);
        }

        public void Dispose()
        {
            m_toRelease.m_semaphore.Release();
        }

        private class FileLock : IDisposable
        {
            private static Dictionary<string, FileLock> _lockedFiles = new Dictionary<string, FileLock>();

            private int? _currentSchedulerId;

            public readonly SemaphoreSlim m_semaphore = new SemaphoreSlim(1, 1);

            private readonly Task<FileLockReleaser> m_releaser;

            private int _referenceCount = 0;

            private string _originalLockedFileName;

            private FileLock(string fullName)
            {
                _originalLockedFileName = fullName;
                m_releaser = Task.FromResult(new FileLockReleaser(this));
            }

            public IDataStore DataStore { get { return New<IDataStore>(_originalLockedFileName); } }

            public Task<FileLockReleaser> LockAsync()
            {
                Task wait = m_semaphore.WaitAsync();
                return wait.IsCompleted ?
                            m_releaser :
                            wait.ContinueWith((_, state) => (FileLockReleaser)state,
                                m_releaser.Result, CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            }

            private FileLockReleaser Lock()
            {
                if (m_semaphore.CurrentCount == 0 && _currentSchedulerId == TaskScheduler.Current?.Id)
                {
                    throw new InternalErrorException($"Potential deadlock detected for {_originalLockedFileName} .");
                }

                m_semaphore.Wait();
                _currentSchedulerId = TaskScheduler.Current?.Id;
                return m_releaser.Result;
            }

            public static FileLockReleaser Lock(IDataItem dataItem)
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
                        return fileLock.Lock();
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
                    FileLock fileLock;
                    if (!_lockedFiles.TryGetValue(fullName, out fileLock))
                    {
                        return false;
                    }

                    return fileLock.m_semaphore.CurrentCount == 0;
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
}
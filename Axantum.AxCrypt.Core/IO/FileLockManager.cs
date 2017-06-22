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

using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.IO
{
    internal class FileLockManager
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private readonly Task<FileLock> _fileLock;

        private readonly TimeSpan _timeout;

        private readonly FileLocker _fileLocker;

        private int _referenceCount = 0;

        private string _originalLockedFileName;

        public FileLockManager(string fullName, TimeSpan timeout, FileLocker fileLocker)
        {
            _originalLockedFileName = fullName;
            _fileLock = Task.FromResult(new FileLock(this));
            _timeout = timeout;
            _fileLocker = fileLocker;
        }

        public IDataStore DataStore { get { return New<IDataStore>(_originalLockedFileName); } }

        public Task<FileLock> LockAsync()
        {
            Interlocked.Increment(ref _referenceCount);
            Task wait = _semaphore.WaitAsync();
            return wait.IsCompleted ?
                        _fileLock :
                        wait.ContinueWith((_, state) => (FileLock)state,
                            _fileLock.Result, CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        public FileLock Lock()
        {
            Interlocked.Increment(ref _referenceCount);
            if (!_semaphore.Wait(_timeout))
            {
                throw new InternalErrorException("Potential deadlock detected.", _originalLockedFileName);
            }

            return _fileLock.Result;
        }

        public int CurrentCount
        {
            get { return _semaphore.CurrentCount; }
        }

        public void Release()
        {
            if (_semaphore.CurrentCount > 0)
            {
                throw new InvalidOperationException($"Call to {nameof(Release)}() without holding the lock.");
            }
            if (Interlocked.Decrement(ref _referenceCount) == 0)
            {
                _fileLocker.Release(_originalLockedFileName);
            }
            _semaphore.Release();
        }
    }
}
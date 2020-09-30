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

using AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.IO
{
    internal sealed class FileLockManager : IDisposable
    {
        private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private readonly TimeSpan _timeout;

        private readonly FileLocker _fileLocker;

        private int _referenceCount = 0;

        private string _originalLockedFileName;

        public FileLockManager(string fullName, TimeSpan timeout, FileLocker fileLocker)
        {
            _originalLockedFileName = fullName;
            _timeout = timeout;
            _fileLocker = fileLocker;
        }

        public IDataStore DataStore { get { return New<IDataStore>(_originalLockedFileName); } }

        public Task<FileLock> GetFileLockAsync()
        {
            FileLock fileLock = new FileLock(this);
            IncrementReferenceCount();
            Task wait = _semaphore.WaitAsync();
            return wait.IsCompleted ?
                        Task.FromResult(fileLock) :
                        wait.ContinueWith((_, state) => (FileLock)state, fileLock, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        public FileLock GetFileLock()
        {
            IncrementReferenceCount();
            if (!_semaphore.Wait(_timeout))
            {
#if DEBUG
                Debugger.Break();
#endif
                throw new InternalErrorException("Potential deadlock detected.", _originalLockedFileName);
            }

            return new FileLock(this);
        }

        public int IncrementReferenceCount()
        {
            return Interlocked.Increment(ref _referenceCount);
        }

        public int DecrementReferenceCount()
        {
            return Interlocked.Decrement(ref _referenceCount);
        }

        public bool IsLocked
        {
            get { return _semaphore.CurrentCount == 0; }
        }

        public void Release()
        {
            if (_semaphore.CurrentCount > 0)
            {
                throw new InvalidOperationException($"Call to {nameof(Release)}() without holding the lock.");
            }

            if (_fileLocker.TryRemove(_originalLockedFileName))
            {
                return;
            }

            _semaphore.Release();
        }

        public void Dispose()
        {
            if (_semaphore != null)
            {
                _semaphore.Dispose();
                _semaphore = null;
            }
        }
    }
}
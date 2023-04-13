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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.IO
{
    public class LockedStream : WrappedBaseStream
    {
        [AllowNull]
        private FileLock _fileLock;

        public static LockedStream OpenWrite(IDataStore dataStore)
        {
            if (dataStore == null)
            {
                throw new ArgumentNullException(nameof(dataStore));
            }

            LockedStream lockedStream = new LockedStream
            {
                _fileLock = New<FileLocker>().Acquire(dataStore),
                WrappedStream = dataStore.OpenWrite()
            };

            return lockedStream;
        }

        public static LockedStream OpenRead(IDataStore dataStore)
        {
            if (dataStore == null)
            {
                throw new ArgumentNullException(nameof(dataStore));
            }

            LockedStream lockedStream = new LockedStream
            {
                _fileLock = New<FileLocker>().Acquire(dataStore),
                WrappedStream = dataStore.OpenRead()
            };

            return lockedStream;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeInternal();
            }
        }

        private void DisposeInternal()
        {
            try
            {
                base.Dispose(true);
            }
            finally
            {
                if (_fileLock != null)
                {
                    _fileLock.Dispose();
                    _fileLock = null;
                }
            }
        }
    }
}

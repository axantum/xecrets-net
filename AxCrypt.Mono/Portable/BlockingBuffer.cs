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
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Core.Portable;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;

namespace AxCrypt.Mono.Portable
{
    public class BlockingBuffer : IBlockingBuffer
    {
        [AllowNull]
        private BlockingCollection<byte[]> _blockingCollection = new BlockingCollection<byte[]>(10);

        public BlockingBuffer()
        {
        }

        public void Put(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (buffer.Length == 0)
            {
                return;
            }

            _blockingCollection.Add(buffer);
        }

        public byte[] Take()
        {
            if (_blockingCollection.TryTake(out byte[]? item, Timeout.Infinite))
            {
                return item;
            }

            return Array.Empty<byte>();
        }

        public void Complete()
        {
            _blockingCollection.CompleteAdding();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeInternal();
            }
        }

        private void DisposeInternal()
        {
            if (_blockingCollection != null)
            {
                _blockingCollection.Dispose();
                _blockingCollection = null;
            }
        }
    }
}

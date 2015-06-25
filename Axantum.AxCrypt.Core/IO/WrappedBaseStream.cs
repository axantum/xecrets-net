#region Coypright and License

/*
 * AxCrypt - Copyright 2015, Svante Seleborg, All Rights Reserved
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.IO
{
    public abstract class WrappedBaseStream : Stream
    {
        private Stream _wrappedStream;

        /// <summary>
        /// Gets or sets the wrapped stream.
        /// </summary>
        /// <value>
        /// The wrapped stream. It will be disposed when this instance is disposed.
        /// </value>
        /// <exception cref="System.ObjectDisposedException"></exception>
        protected Stream WrappedStream
        {
            get
            {
                if (_wrappedStream == null)
                {
                    throw new ObjectDisposedException(GetType().ToString());
                }
                return _wrappedStream;
            }
            set
            {
                _wrappedStream = value;
            }
        }

        protected bool IsDisposed
        {
            get
            {
                return _wrappedStream == null;
            }
        }

        public override bool CanRead
        {
            get { return WrappedStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return WrappedStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return WrappedStream.CanWrite; }
        }

        public override void Flush()
        {
            WrappedStream.Flush();
        }

        public override long Length
        {
            get { return WrappedStream.Length; }
        }

        public override long Position
        {
            get
            {
                return WrappedStream.Position;
            }
            set
            {
                WrappedStream.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return WrappedStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return WrappedStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            WrappedStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            WrappedStream.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeInternal();
            }
            base.Dispose(disposing);
        }

        private void DisposeInternal()
        {
            if (_wrappedStream != null)
            {
                _wrappedStream.Dispose();
                _wrappedStream = null;
            }
        }
    }
}
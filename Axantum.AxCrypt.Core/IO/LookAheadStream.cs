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

using System;
using System.Collections.Generic;
using System.IO;

namespace Axantum.AxCrypt.Core.IO
{
    /// <summary>
    /// A stream wrapper with push back capability, thus enabling look ahead in the stream.
    /// </summary>
    public class LookAheadStream : Stream
    {
        private Stream _inputStream;

        private bool _disposed = false;

        private Stack<ByteBuffer> _pushBack = new Stack<ByteBuffer>();

        /// <summary>
        /// Implement a stream wrapper with push back capability thus enabling look ahead.
        /// </summary>
        /// <param name="inputStream">The stream. Will be disposed when this instance is disposed.</param>
        public LookAheadStream(Stream inputStream)
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException("inputStream");
            }
            if (!inputStream.CanRead)
            {
                throw new ArgumentException("inputStream must be readable.");
            }
            _inputStream = inputStream;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public void Pushback(byte[] buffer, int offset, int length)
        {
            EnsureNotDisposed();
            byte[] pushbackBuffer = new byte[length];
            Array.Copy(buffer, offset, pushbackBuffer, 0, length);
            _pushBack.Push(new ByteBuffer(pushbackBuffer));
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureNotDisposed();
            int bytesRead = 0;
            while (count > 0 && _pushBack.Count > 0)
            {
                ByteBuffer byteBuffer = _pushBack.Pop();
                int length = byteBuffer.Read(buffer, offset, count);
                offset += length;
                count -= length;
                bytesRead += length;
                if (byteBuffer.AvailableForRead > 0)
                {
                    _pushBack.Push(byteBuffer);
                }
            }
            bytesRead += _inputStream.Read(buffer, offset, count);
            return bytesRead;
        }

        public bool ReadExact(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            int bytesRead = Read(buffer, 0, buffer.Length);

            return bytesRead == buffer.Length;
        }

        /// <summary>
        /// Check if any more data is available in the stream.
        /// </summary>
        /// <param name="suggestedNextReadSize">A guess as to the read, optimizes buffering if correct.</param>
        /// <returns>True if no more bytes can be read from the stream, false otherwise.</returns>
        public bool IsEmpty(int suggestedNextReadSize)
        {
            if (_pushBack.Count > 0)
            {
                return false;
            }
            byte[] buffer = new byte[suggestedNextReadSize];
            int count = Read(buffer, 0, buffer.Length);
            if (count > 0)
            {
                ByteBuffer ahead = new ByteBuffer(buffer);
                ahead.AvailableForRead = count;
                _pushBack.Push(ahead);
                return false;
            }
            return true;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
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
            if (_inputStream != null)
            {
                _inputStream.Dispose();
                _inputStream = null;
            }
            _disposed = true;
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}
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

using AxCrypt.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace AxCrypt.Core.IO
{
    /// <summary>
    /// A stream wrapper with push back capability, thus enabling look ahead in the stream.
    /// </summary>
    public class LookAheadStream : Stream
    {
        [AllowNull]
        private Stream _inputStream;

        private bool _disposed = false;

        private readonly Stack<ByteBuffer> _pushBack = new Stack<ByteBuffer>();

        /// <summary>
        /// Implement a stream wrapper with push back capability thus enabling look ahead.
        /// </summary>
        /// <param name="inputStream">The stream. Will be disposed when this instance is disposed.</param>
        public LookAheadStream(Stream inputStream)
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException(nameof(inputStream));
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

        /// <summary>
        /// Locates the specified buffer content in the stream, if there.
        /// </summary>
        /// <param name="buffer">The buffer to find.</param>
        /// <returns>True if found, and then positioned just after. False if not found and positioned at end of file or just before it.</returns>
        public bool Locate(byte[] pattern)
        {
            byte[] buffer = new byte[OS.Current.StreamBufferSize];
            while (true)
            {
                int bytesRead = Read(buffer, 0, buffer.Length);
                if (bytesRead < pattern.Length)
                {
                    Pushback(buffer, 0, bytesRead);
                    return false;
                }

                int i = buffer.Locate(pattern, 0, bytesRead);
                if (i < 0)
                {
                    int offsetToBytesToKeep = bytesRead - pattern.Length + 1;
                    Pushback(buffer, offsetToBytesToKeep, bytesRead - offsetToBytesToKeep);
                    continue;
                }
                int offsetJustAfterTheGuid = i + pattern.Length;
                Pushback(buffer, offsetJustAfterTheGuid, bytesRead - offsetJustAfterTheGuid);
                return true;
            }
        }

        /// <summary>
        /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>
        /// The total number of bytes read into the buffer.
        /// </returns>
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
                throw new ArgumentNullException(nameof(buffer));
            }

            if (buffer.Length == 0)
            {
                return true;
            }

            int read = 0;
            do
            {
                int bytesRead = Read(buffer, read, buffer.Length - read);
                if (bytesRead <= 0)
                {
                    return false;
                }
                read += bytesRead;
            } while (read < buffer.Length);

            return true;
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
                ByteBuffer ahead = new ByteBuffer(buffer)
                {
                    AvailableForRead = count
                };
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

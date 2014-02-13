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

using System;
using System.IO;
using System.Security.Cryptography;

namespace Axantum.AxCrypt.Core.IO
{
    public class V2HmacStream : Stream
    {
        private HashAlgorithm _hmac;

        public Stream ChainedStream { get; protected set; }

        private long _count = 0;

        private bool _disposed = false;

        /// <summary>
        /// A AxCrypt HMAC-calculating stream.
        /// </summary>
        /// <param name="key">The key for the HMAC</param>
        public V2HmacStream(byte[] key)
            : this(key, Stream.Null)
        {
        }

        /// <summary>
        /// An AxCrypt HMAC-SHA-512-calculating stream.
        /// </summary>
        /// <param name="key">The key for the HMAC</param>
        /// <param name="chainedStream">A stream where data is chain-written to. This stream is not disposed of when this instance is disposed.</param>
        public V2HmacStream(byte[] key, Stream chainedStream)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            _hmac = new HMACSHA512(key);

            ChainedStream = chainedStream;
        }

        private byte[] _hmacResult = null;

        /// <summary>
        /// Get the calculated HMAC
        /// </summary>
        /// <returns>The HMAC</returns>
        public byte[] HmacResult
        {
            get
            {
                EnsureNotDisposed();
                if (_hmacResult == null)
                {
                    _hmac.TransformFinalBlock(new byte[] { }, 0, 0);
                    byte[] result = new byte[_hmac.HashSize / 8];
                    Array.Copy(_hmac.Hash, 0, result, 0, result.Length);
                    _hmacResult = result;
                }
                return _hmacResult;
            }
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { return _count; }
        }

        public override long Position
        {
            get
            {
                return _count;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
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
            EnsureNotDisposed();
            WriteInternal(buffer, offset, count);
            ChainedStream.Write(buffer, offset, count);
        }

        private void WriteInternal(byte[] buffer, int offset, int count)
        {
            if (_hmacResult != null)
            {
                throw new InvalidOperationException("Cannot add to the HMAC once it has been finalized.");
            }
            _hmac.TransformBlock(buffer, offset, count, null, 0);
            _count += count;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_disposed)
                {
                    return;
                }
                if (_hmac != null)
                {
                    _hmac.Clear();
                    _hmac = null;
                }
                _disposed = true;
            }
            base.Dispose(disposing);
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
#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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

namespace Axantum.AxCrypt.Core.IO
{
    /// <summary>
    /// Present an input stream as having an exact limit on it's size, although the input
    /// stream in fact may be longer.
    /// </summary>
    /// <remarks>Does not dispose of the input stream or the hmac stream when disposed!</remarks>
    public class AxCryptDataStream : Stream
    {
        private Stream _inputStream;
        private Stream _hmacStream;
        private long _length;

        private long _remaining;

        /// <summary>
        /// Wrap a general stream to serve as a stream for AxCrypt data, limited in length
        /// and optionally sending the data to a presumed hmacStream.
        /// </summary>
        /// <param name="inputStream">A stream positioned at the first byte of data</param>
        /// <param name="hmacStream">A stream where all data read is mirrored, presumably to calculate an HMAC. If null, ignored.</param>
        /// <param name="length">The exact number of bytes to expect and read from the input stream</param>
        public AxCryptDataStream(Stream inputStream, Stream hmacStream, long length)
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException("inputStream");
            }
            if (hmacStream == null)
            {
                throw new ArgumentNullException("hmacStream");
            }
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }
            _inputStream = inputStream;
            _hmacStream = hmacStream;
            _length = length;
            _remaining = _length;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Length
        {
            get { return _length; }
        }

        public override long Position
        {
            get
            {
                return _length - _remaining;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_remaining == 0)
            {
                return 0;
            }

            int bytesToRead = _remaining < count ? (int)_remaining : count;
            int bytesRead = _inputStream.Read(buffer, offset, bytesToRead);
            _remaining -= bytesRead;

            _hmacStream.Write(buffer, 0, bytesRead);

            return bytesRead;
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
    }
}
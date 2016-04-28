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
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.IO
{
    public class ByteBuffer
    {
        private byte[] _buffer;

        private int _nextReadOffset;

        private int _nextWriteOffset;

        public ByteBuffer(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            _buffer = buffer;
            _nextWriteOffset = buffer.Length;
        }

        public int AvailableForRead
        {
            get
            {
                return _nextWriteOffset - _nextReadOffset;
            }
            set
            {
                _nextReadOffset = 0;
                _nextWriteOffset = value;
            }
        }

        public int AvailableForWrite
        {
            get
            {
                return _buffer.Length - _nextWriteOffset;
            }
            set
            {
                _nextReadOffset = _nextWriteOffset = _buffer.Length - value;
            }
        }

        public int Write(byte[] inBuffer, int inOffset, int inCount)
        {
            int bytesToCopy = inCount > AvailableForWrite ? AvailableForWrite : inCount;
            Array.Copy(inBuffer, inOffset, _buffer, _nextWriteOffset, bytesToCopy);
            _nextWriteOffset += bytesToCopy;
            return bytesToCopy;
        }

        public int Read(byte[] outBuffer, int outOffset, int outCount)
        {
            int bytesToCopy = outCount > AvailableForRead ? AvailableForRead : outCount;
            Array.Copy(_buffer, _nextReadOffset, outBuffer, outOffset, bytesToCopy);
            _nextReadOffset += bytesToCopy;
            return bytesToCopy;
        }

        public byte[] GetBuffer()
        {
            return _buffer;
        }

        public int Length
        {
            get
            {
                return _buffer.Length;
            }
        }
    }
}
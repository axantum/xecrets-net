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

using Axantum.AxCrypt.Core.Header;
using Axantum.AxCrypt.Core.Reader;
using Axantum.AxCrypt.Core.Runtime;
using System;
using System.IO;
using System.Linq;

namespace Axantum.AxCrypt.Core.IO
{
    /// <summary>
    /// Read and Write data to an encrypted data stream, wrapping the data during write in EncryptedDataPartBlock
    /// blocks. During read, interpret and strip the EncryptedDataPartBlock structure, returning raw data.
    /// </summary>
    public class V2AxCryptDataStream : Stream
    {
        public static readonly int WriteChunkSize = 65536;

        private AxCryptReader _reader;

        private Stream _hmacStream;

        private byte[] _buffer;

        private int _offset;

        /// <summary>
        /// Instantiate an instance of a stream to write to.
        /// </summary>
        /// <param name="hmacStream">A stream to pass all data to, typically to calculate an HMAC.</param>
        public V2AxCryptDataStream(Stream hmacStream)
        {
            _hmacStream = hmacStream;
        }

        /// <summary>
        /// Instantiate an instance of a stream to read from.
        /// </summary>
        /// <param name="reader">An AxCrypt reader where EnryptedDataPartBlock parts are read from.</param>
        /// <param name="hmacStream">A stream to pass all data to, typically to calculate an HMAC.</param>
        public V2AxCryptDataStream(AxCryptReader reader, Stream hmacStream)
            : this(hmacStream)
        {
            _reader = reader;
        }

        public override bool CanRead
        {
            get { return _reader != null; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return _hmacStream.CanWrite; }
        }

        public override void Flush()
        {
            if (CanRead || _buffer == null || _offset == 0)
            {
                return;
            }

            byte[] buffer = _buffer;
            if (_offset != _buffer.Length)
            {
                byte[] partBuffer = new byte[_offset];
                Array.Copy(_buffer, 0, partBuffer, 0, _offset);
                buffer = partBuffer;
            }
            _offset = 0;

            EncryptedDataPartBlock dataPart = new EncryptedDataPartBlock(buffer);
            dataPart.Write(_hmacStream);
        }

        public override void Close()
        {
            Flush();
            base.Close();
        }

        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
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

        private bool _eof = false;

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_eof || !DataInBuffer())
            {
                _eof = true;
                return 0;
            }

            int available = _buffer.Length - _offset;
            int read = count > available ? available : count;

            Array.Copy(_buffer, _offset, buffer, offset, read);

            _offset += read;
            return read;
        }

        private bool DataInBuffer()
        {
            if (_buffer != null && _offset < _buffer.Length)
            {
                return true;
            }

            if (!ReadFromReader())
            {
                return false;
            }

            EncryptedDataPartBlock dataPart = (EncryptedDataPartBlock)_reader.CurrentHeaderBlock;
            _buffer = dataPart.GetDataBlockBytes();
            _offset = 0;
            return true;
        }

        private bool ReadFromReader()
        {
            if (!_reader.Read())
            {
                throw new FileFormatException("Unexpected end of file during read of encrypted data stream.");
            }

            if (_reader.CurrentItemType != AxCryptItemType.HeaderBlock)
            {
                throw new FileFormatException("Unexpected block type encountered during read of encrypted data stream.");
            }

            if (_reader.CurrentHeaderBlock.HeaderBlockType != HeaderBlockType.EncryptedDataPart)
            {
                return false;
            }

            _reader.CurrentHeaderBlock.Write(_hmacStream);
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
            while (count > 0)
            {
                int written = WriteToBuffer(buffer, offset, count);
                count -= written;
                offset += written;
            }
        }

        private int WriteToBuffer(byte[] buffer, int offset, int count)
        {
            if (_buffer == null)
            {
                _buffer = new byte[WriteChunkSize];
                _offset = 0;
            }

            int room = _buffer.Length - _offset;
            int written = room > count ? count : room;
            Array.Copy(buffer, offset, _buffer, _offset, written);

            _offset += written;
            if (_offset == _buffer.Length)
            {
                Flush();
            }

            return written;
        }
    }
}
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
    public sealed class V2AxCryptDataStream : V2AxCryptDataStream<Stream>
    {
        /// <summary>
        /// The suggest default write chunk size
        /// </summary>
        public static readonly int WriteChunkSize = 65536;

        private V2AxCryptDataStream(AxCryptReaderBase reader, Stream chainedStream)
            : base(reader, chainedStream)
        {
        }

        /// <summary>
        /// Creates the specified data stream for reading from, ensuring proper disposal.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="chainedStream">The chained stream.</param>
        /// <returns></returns>
        public static V2AxCryptDataStream Create(AxCryptReaderBase reader, Stream chainedStream)
        {
            return V2AxCryptDataStream<Stream>.Create((chained) => new V2AxCryptDataStream(reader, chainedStream), chainedStream);
        }

        /// <summary>
        /// Creates the specified data stream for writing to, ensuring proper disposal.
        /// </summary>
        /// <param name="chainedStream">The chained stream.</param>
        /// <returns></returns>
        public static V2AxCryptDataStream Create(Stream chainedStream)
        {
            return Create(null, chainedStream);
        }
    }

    /// <summary>
    /// Read and Write data to an encrypted data stream, wrapping the data during write in EncryptedDataPartBlock
    /// blocks. During read, interpret and strip the EncryptedDataPartBlock structure, returning raw data. This class is
    /// for internal use only, and is not intended to be instantiated directly.
    /// </summary>
    public abstract class V2AxCryptDataStream<T> : ChainedStream<T> where T : Stream
    {
        private AxCryptReaderBase _reader;

        private byte[] _buffer;

        private int _offset;

        /// <summary>
        /// Instantiate an instance of a stream to read or write from.
        /// </summary>
        /// <param name="reader">An AxCrypt reader where EnryptedDataPartBlock parts are read from. If null, the stream is intended for writing.</param>
        /// <param name="chained">A stream to read blocked data from or write blocks to. It will be disposed of.</param>
        protected V2AxCryptDataStream(AxCryptReaderBase reader, T chained)
            : base(chained)
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
            get { return Chained.CanWrite; }
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
            dataPart.Write(Chained);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Flush();
            }
            base.Dispose(disposing);
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
            int read = 0;
            while (read < count)
            {
                if (_eof || !DataInBuffer())
                {
                    _eof = true;
                    return read;
                }
                int available = _buffer.Length - _offset;
                int thisread = (count - read) > available ? available : count - read;

                Array.Copy(_buffer, _offset, buffer, offset, thisread);
                offset += thisread;
                _offset += thisread;
                read += thisread;
            }
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

            _reader.CurrentHeaderBlock.Write(Chained);
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
                _buffer = new byte[V2AxCryptDataStream.WriteChunkSize];
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
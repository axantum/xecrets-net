using Axantum.AxCrypt.Core.Header;
using Axantum.AxCrypt.Core.Reader;
using System;
using System.IO;
using System.Linq;

namespace Axantum.AxCrypt.Core.IO
{
    public class V2AxCryptDataStream : Stream
    {
        private const int WRITE_CHUNK_SIZE = 65536;

        private AxCryptReader _reader;

        private Stream _hmacStream;

        private byte[] _buffer;

        private int _offset;

        public V2AxCryptDataStream(Stream hmacStream)
        {
            _hmacStream = hmacStream;
        }

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
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!DataInBuffer())
            {
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
                return false;
            }

            if (_reader.CurrentItemType != AxCryptItemType.HeaderBlock)
            {
                return false;
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
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
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
                _buffer = new byte[WRITE_CHUNK_SIZE];
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
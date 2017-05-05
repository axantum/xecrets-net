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

using Axantum.AxCrypt.Abstractions.Algorithm;
using Axantum.AxCrypt.Core.Algorithm;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Portable;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Algorithm
{
    public class CryptoTransformingStream : CryptoStream
    {
        private Stream _outStream;

        private LookAheadStream _inStream;

        private ICryptoTransform _transform;

        private ByteBuffer _blockBuffer;

        public CryptoTransformingStream()
        {
        }

        public override CryptoStream Initialize(Stream stream, ICryptoTransform transform, CryptoStreamMode mode)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (transform == null)
            {
                throw new ArgumentNullException("transform");
            }
            if (transform.InputBlockSize != transform.OutputBlockSize)
            {
                throw new ArgumentException("The transform must have the same input and output block size.", "transform");
            }

            _transform = transform;

            if (mode == CryptoStreamMode.Read)
            {
                _inStream = new LookAheadStream(stream);
            }
            else
            {
                _outStream = stream;
            }

            _blockBuffer = new ByteBuffer(new byte[_transform.InputBlockSize]);
            _blockBuffer.AvailableForRead = 0;

            return this;
        }

        private Stream Stream
        {
            get { return _inStream ?? _outStream; }
        }

        public override bool CanRead
        {
            get { return Stream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return Stream.CanWrite; }
        }

        public override void Flush()
        {
            Stream.Flush();
        }

        private bool _hasFinalFlushed = false;

        public override void FinalFlush()
        {
            if (_hasFinalFlushed)
            {
                throw new NotSupportedException("Final flush was called multiple times. This is not supported.");
            }
            byte[] block = _transform.TransformFinalBlock(_blockBuffer.GetBuffer(), 0, _blockBuffer.AvailableForRead);
            _outStream.Write(block, 0, block.Length);
            _outStream.Flush();
            _blockBuffer.AvailableForWrite = _blockBuffer.Length;
            _hasFinalFlushed = true;
        }

        public override long Length
        {
            get { return Stream.Length; }
        }

        public override long Position
        {
            get
            {
                return Stream.Position;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int originalCount = count;
            while (count > 0)
            {
                int bytesRead = _blockBuffer.Read(buffer, offset, count);
                if (bytesRead > 0)
                {
                    offset += bytesRead;
                    count -= bytesRead;
                    continue;
                }

                if (_inStream.IsEmpty(_blockBuffer.Length))
                {
                    break;
                }

                byte[] block = new byte[_blockBuffer.Length];
                bytesRead = _inStream.Read(block, 0, block.Length);
                if (_inStream.IsEmpty(_blockBuffer.Length))
                {
                    byte[] finalOutput = _transform.TransformFinalBlock(block, 0, bytesRead);
                    _blockBuffer = new ByteBuffer(finalOutput);
                    continue;
                }

                _blockBuffer.AvailableForWrite = _blockBuffer.Length;
                int bytesTransformed = _transform.TransformBlock(block, 0, bytesRead, _blockBuffer.GetBuffer(), 0);
                _blockBuffer.AvailableForRead = bytesTransformed;
            }
            return originalCount - count;
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
                if (_blockBuffer.AvailableForRead == _blockBuffer.Length)
                {
                    byte[] block = new byte[_blockBuffer.Length];
                    int written = _transform.TransformBlock(_blockBuffer.GetBuffer(), 0, _blockBuffer.Length, block, 0);
                    _outStream.Write(block, 0, written);
                    _blockBuffer.AvailableForWrite = _blockBuffer.Length;
                }

                int bytesWritten = _blockBuffer.Write(buffer, offset, count);
                offset += bytesWritten;
                count -= bytesWritten;
            }
        }

        public override bool CanTimeout
        {
            get
            {
                return Stream.CanTimeout;
            }
        }

        public override bool Equals(object obj)
        {
            return Stream.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Stream.GetHashCode();
        }

        public override int ReadByte()
        {
            byte[] oneByte = new byte[1];
            int bytesRead = Read(oneByte, 0, 1);
            if (bytesRead == 1)
            {
                return oneByte[0];
            }
            return -1;
        }

        public override void WriteByte(byte value)
        {
            byte[] oneByte = new byte[] { value };
            Write(oneByte, 0, 1);
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
            if (_outStream != null)
            {
                if (!_hasFinalFlushed)
                {
                    FinalFlush();
                }

                _outStream.Dispose();
                _outStream = null;
            }

            if (_inStream != null)
            {
                _inStream.Dispose();
                _inStream = null;
            }

            if (_transform != null)
            {
                _transform.Dispose();
                _transform = null;
            }
        }
    }
}
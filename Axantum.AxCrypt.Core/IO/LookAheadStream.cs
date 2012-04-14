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
using System.Collections.Generic;
using System.IO;

namespace Axantum.AxCrypt.Core.IO
{
    public class LookAheadStream : Stream
    {
        private struct ByteBuffer
        {
            public ByteBuffer(byte[] buffer, int offset, int length)
            {
                Buffer = buffer;
                Offset = offset;
                Length = length;
            }

            public byte[] Buffer;
            public int Offset;
            public int Length;
        }

        private Stream InputStream { get; set; }

        private Stack<ByteBuffer> pushBack = new Stack<ByteBuffer>();

        /// <summary>
        /// Implement a stream with push back capability thus enabling look ahead.
        /// </summary>
        /// <param name="inputStream">The stream. Will be disposed when this instance is disposed.</param>
        public LookAheadStream(Stream inputStream)
        {
            InputStream = inputStream;
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
            pushBack.Push(new ByteBuffer(buffer, offset, length));
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = 0;
            while (count > 0 && pushBack.Count > 0)
            {
                ByteBuffer byteBuffer = pushBack.Pop();
                int length = byteBuffer.Length >= count ? count : byteBuffer.Length;
                Array.Copy(byteBuffer.Buffer, byteBuffer.Offset, buffer, offset, length);
                offset += length;
                count -= length;
                byteBuffer.Length -= length;
                byteBuffer.Offset += length;
                bytesRead += length;
                if (byteBuffer.Length > 0)
                {
                    pushBack.Push(byteBuffer);
                }
            }
            if (count > 0)
            {
                bytesRead += InputStream.Read(buffer, offset, count);
            }
            return bytesRead;
        }

        public bool ReadExact(byte[] buffer)
        {
            int bytesRead = Read(buffer, 0, buffer.Length);

            return bytesRead == buffer.Length;
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
                if (InputStream != null)
                {
                    InputStream.Dispose();
                    InputStream = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}
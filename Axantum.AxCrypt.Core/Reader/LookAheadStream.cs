using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Reader
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
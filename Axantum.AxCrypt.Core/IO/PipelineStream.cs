using Axantum.AxCrypt.Core.Portable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.IO
{
    public class PipelineStream : Stream
    {
        private IBlockingBuffer _blockingBuffer;

        private ByteBuffer _overflowBuffer = new ByteBuffer(new byte[0]);

        public PipelineStream()
        {
            _blockingBuffer = Resolve.Portable.BlockingBuffer();
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
            get { return true; }
        }

        public override void Flush()
        {
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
            if (_overflowBuffer.AvailableForRead == 0)
            {
                _overflowBuffer = new ByteBuffer(_blockingBuffer.Take());
            }

            return _overflowBuffer.Read(buffer, offset, count);
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
            byte[] copy = new byte[count];
            Array.Copy(buffer, offset, copy, 0, count);

            _blockingBuffer.Put(copy);
        }

        public void Complete()
        {
            _blockingBuffer.Complete();
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
            if (_blockingBuffer != null)
            {
                _blockingBuffer.Dispose();
                _blockingBuffer = null;
            }
        }
    }
}
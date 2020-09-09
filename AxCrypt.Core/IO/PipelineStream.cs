using AxCrypt.Core.Portable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace AxCrypt.Core.IO
{
    public class PipelineStream : Stream
    {
        private CancellationToken _cancellationToken;

        private IBlockingBuffer _blockingBuffer;

        private ByteBuffer _overflowBuffer = new ByteBuffer(new byte[0]);

        public PipelineStream(CancellationToken cancellationToken)
        {
            _blockingBuffer = Resolve.Portable.BlockingBuffer();
            _cancellationToken = cancellationToken;
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

        public override int Read(byte[] buffer, int offset, int count)
        {
            CheckCancellation();
            if (_overflowBuffer.AvailableForRead == 0)
            {
                _overflowBuffer = new ByteBuffer(_blockingBuffer.Take());
            }

            return _overflowBuffer.Read(buffer, offset, count);
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
            CheckCancellation();
            byte[] copy = new byte[count];
            Array.Copy(buffer, offset, copy, 0, count);

            _blockingBuffer.Put(copy);
        }

        public void Complete()
        {
            CheckCancellation();
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

        private void CheckCancellation()
        {
            if (_cancellationToken.IsCancellationRequested)
            {
                _cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.UI;

namespace Axantum.AxCrypt.Core.IO
{
    public class ProgressStream : Stream
    {
        private Stream _stream;

        private ProgressContext _progress;

        public ProgressStream(Stream stream, ProgressContext progress)
        {
            _stream = stream;
            _progress = progress;

            _progress.Max = _stream.CanSeek ? _stream.Length : -1;
        }

        public override bool CanRead
        {
            get { return _stream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _stream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return _stream.CanWrite; }
        }

        public override void Flush()
        {
            _stream.Flush();
        }

        public override long Length
        {
            get { return _stream.Length; }
        }

        public override long Position
        {
            get
            {
                return _stream.Position;
            }
            set
            {
                _stream.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytes = _stream.Read(buffer, offset, count);

            _progress.Current = _stream.CanSeek ? _stream.Position : -1;
            return bytes;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);

            _progress.Current = _stream.CanSeek ? _stream.Position : -1;
        }

        private bool _disposed = false;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_disposed)
                {
                    return;
                }
                if (_stream != null)
                {
                    _stream.Close();
                    _stream = null;
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }
    }
}
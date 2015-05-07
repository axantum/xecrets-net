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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Test
{
    internal class FakeStream : Stream
    {
        private Action<string> _visitorAction;
        private Stream _stream;

        public FakeStream(Stream stream, Action<string> visitorAction)
        {
            _stream = stream;
            _visitorAction = visitorAction;
        }

        public override bool CanRead
        {
            get
            {
                _visitorAction("CanRead");
                return _stream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                _visitorAction("CanSeek");
                return _stream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                _visitorAction("CanWrite");
                return _stream.CanWrite;
            }
        }

        public override void Flush()
        {
            _visitorAction("Flush");
            _stream.Flush();
        }

        public override long Length
        {
            get
            {
                _visitorAction("Length");
                return _stream.Length;
            }
        }

        public override long Position
        {
            get
            {
                _visitorAction("getPosition");
                return _stream.Position;
            }
            set
            {
                _visitorAction("setPosition");
                _stream.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            _visitorAction("Read");
            return _stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            _visitorAction("Seek");
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _visitorAction("SetLength");
            _stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _visitorAction("Write");
            _stream.Write(buffer, offset, count);
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
            if (_stream != null)
            {
                _stream.Close();
                _stream = null;
            }
        }
    }
}
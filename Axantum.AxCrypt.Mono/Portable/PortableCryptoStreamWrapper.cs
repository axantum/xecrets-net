using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Mono.Portable
{
    public class PortableCryptoStreamWrapper : Axantum.AxCrypt.Core.Portable.CryptoStream
    {
        private System.Security.Cryptography.CryptoStream _cryptoStream;

        public PortableCryptoStreamWrapper(System.Security.Cryptography.CryptoStream cryptoStream)
        {
            _cryptoStream = cryptoStream;
        }

        public override bool CanRead
        {
            get { return _cryptoStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _cryptoStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return _cryptoStream.CanWrite; }
        }

        public override void Flush()
        {
            _cryptoStream.Flush();
        }

        public override long Length
        {
            get { return _cryptoStream.Length; }
        }

        public override long Position
        {
            get
            {
                return _cryptoStream.Position;
            }
            set
            {
                _cryptoStream.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _cryptoStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _cryptoStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _cryptoStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _cryptoStream.Write(buffer, offset, count);
        }

        public override bool CanTimeout
        {
            get
            {
                return _cryptoStream.CanTimeout;
            }
        }

        public override void Close()
        {
            _cryptoStream.Close();
        }

        public override bool Equals(object obj)
        {
            return _cryptoStream.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _cryptoStream.GetHashCode();
        }

        public override int ReadByte()
        {
            return _cryptoStream.ReadByte();
        }

        public override void WriteByte(byte value)
        {
            _cryptoStream.WriteByte(value);
        }
    }
}
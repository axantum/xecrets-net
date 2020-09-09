using AxCrypt.Abstractions.Algorithm;
using AxCrypt.Core.Algorithm;
using AxCrypt.Core.Portable;
using AxCrypt.Mono.Portable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AxCrypt.Mono.Cryptography
{
    internal class CryptoStreamWrapper : CryptoStreamBase
    {
        private System.Security.Cryptography.CryptoStream _cryptoStream;

        private System.Security.Cryptography.ICryptoTransform _cryptoTransform;

        public override CryptoStreamBase Initialize(Stream stream, ICryptoTransform transform, CryptoStreamMode mode)
        {
            _cryptoTransform = new CryptoTransformUnwrapper(transform);

            System.Security.Cryptography.CryptoStreamMode streamMode;
            switch (mode)
            {
                case CryptoStreamMode.Read:
                    streamMode = System.Security.Cryptography.CryptoStreamMode.Read;
                    break;

                case CryptoStreamMode.Write:
                    streamMode = System.Security.Cryptography.CryptoStreamMode.Write;
                    break;

                default:
                    streamMode = (System.Security.Cryptography.CryptoStreamMode)mode;
                    break;
            }

            _cryptoStream = new System.Security.Cryptography.CryptoStream(stream, _cryptoTransform, streamMode);
            return this;
        }

        public CryptoStreamWrapper()
        {
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

        public override void FinalFlush()
        {
            _cryptoStream.FlushFinalBlock();
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

        protected override void Dispose(bool disposing)
        {
            if (_cryptoStream != null)
            {
                _cryptoStream.Dispose();
                _cryptoStream = null;
            }
            if (_cryptoTransform != null)
            {
                _cryptoTransform.Dispose();
                _cryptoTransform = null;
            }
            base.Dispose(disposing);
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.IO;

namespace Axantum.AxCrypt.Core.Test
{
    internal class FakeRuntimeEnvironment : IRuntimeEnvironment
    {
        private byte _randomForTest = 0;

        private bool _isLittleEndian = BitConverter.IsLittleEndian;

        public FakeRuntimeEnvironment()
        {
        }

        public FakeRuntimeEnvironment(Endian endianness)
        {
            _isLittleEndian = endianness == Endian.Reverse ? !_isLittleEndian : _isLittleEndian;
        }

        public bool IsLittleEndian
        {
            get { return _isLittleEndian; }
        }

        public byte[] GetRandomBytes(int count)
        {
            byte[] bytes = new byte[count];
            for (int i = 0; i < count; ++i)
            {
                bytes[i] = _randomForTest++;
            }
            return bytes;
        }

        public IRuntimeFileInfo FileInfo(FileInfo file)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
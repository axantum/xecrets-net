using Axantum.AxCrypt.Core.Crypto;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    internal class FakeRandomGenerator : IRandomGenerator
    {
        private byte _randomForTest = 0;

        public byte[] Generate(int count)
        {
            byte[] bytes = new byte[count];
            for (int i = 0; i < count; ++i)
            {
                bytes[i] = _randomForTest++;
            }
            return bytes;
        }
    }
}
using System;
using System.Linq;
using System.Security.Cryptography;

namespace Axantum.AxCrypt.Core.Crypto
{
    public class RandomGenerator : IRandomGenerator
    {
        private RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        public byte[] Generate(int count)
        {
            byte[] data = new byte[count];
            _rng.GetBytes(data);
            return data;
        }
    }
}
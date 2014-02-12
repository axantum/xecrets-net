using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Crypto
{
    public interface IRandomGenerator
    {
        byte[] Generate(int count);
    }
}
using System;
using System.Linq;

namespace AxCrypt.Core.Crypto
{
    public interface IRandomGenerator
    {
        byte[] Generate(int count);
    }
}
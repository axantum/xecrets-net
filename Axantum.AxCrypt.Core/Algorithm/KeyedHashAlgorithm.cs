using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Algorithm
{
    public abstract class KeyedHashAlgorithm : HashAlgorithm
    {
        public abstract byte[] Key { get; set; }
    }
}
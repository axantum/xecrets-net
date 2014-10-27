using Axantum.AxCrypt.Core.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Portable
{
    public abstract class HMAC : KeyedHashAlgorithm
    {
        protected int BlockSizeValue { get; set; }

        public abstract string HashName { get; set; }

        public abstract HMAC Initialize(SymmetricKey key);
    }
}
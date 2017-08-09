using System;
using System.Collections.Generic;
using System.Linq;

namespace Axantum.AxCrypt.Core.Crypto
{
    public class WebUserIterationCalculator : IterationCalculator
    {
        public override long KeyWrapIterations(Guid cryptoId)
        {
            return 10000;
        }
    }
}
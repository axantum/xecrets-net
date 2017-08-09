using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

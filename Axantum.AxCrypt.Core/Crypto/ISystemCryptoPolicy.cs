using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.Crypto
{
    public interface ISystemCryptoPolicy
    {
        int Priority { get; }

        bool Active { get; }

        string Name { get; }

        ICryptoFactory PreferredCryptoFactory(IEnumerable<CryptoFactoryCreator> factories);
    }
}
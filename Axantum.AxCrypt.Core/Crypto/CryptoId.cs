using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Axantum.AxCrypt.Core.Crypto
{
    public enum CryptoId
    {
        Unknown,

        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "To be fixed when pluggable ICrypto implementations are supported.")]
        Aes_128_V1 = 1,

        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "To be fixed when pluggable ICrypto implementations are supported.")]
        Aes_256 = 2,
    }
}
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Core.Runtime
{
    public interface ILicenseAuthority
    {
        Task<IAsymmetricPrivateKey> PrivateKeyAsync();

        Task<IAsymmetricPublicKey> PublicKeyAsync();
    }
}
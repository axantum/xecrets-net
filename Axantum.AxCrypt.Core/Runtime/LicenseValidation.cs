using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Runtime
{
    public class LicenseValidation
    {
        public async Task<string> SignAsync(UserAccount ua)
        {
            if (ua.SubscriptionLevel != SubscriptionLevel.Premium)
            {
                return string.Empty;
            }

            string email = ua.UserName;
            string level = ua.SubscriptionLevel.ToString();
            string expiration = ua.LevelExpiration.ToString("u");

            IAsymmetricPrivateKey privateKey = await New<ILicenseAuthority>().PrivateKeyAsync();

            byte[] signature = new Signer(privateKey).Sign(email, level, expiration);

            return Convert.ToBase64String(signature);
        }
    }
}
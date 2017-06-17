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
            if (ua == null)
            {
                throw new ArgumentNullException(nameof(ua));
            }

            if (ua.SubscriptionLevel != SubscriptionLevel.Premium)
            {
                return string.Empty;
            }

            IAsymmetricPrivateKey privateKey = await New<ILicenseAuthority>().PrivateKeyAsync();
            byte[] signature = new Signer(privateKey).Sign(SignedFields(ua));

            return Convert.ToBase64String(signature);
        }

        public async Task<SubscriptionLevel> ValidateLevelAsync(UserAccount userAccount)
        {
            if (userAccount == null)
            {
                throw new ArgumentNullException(nameof(userAccount));
            }

            if (userAccount.SubscriptionLevel != SubscriptionLevel.Premium)
            {
                return userAccount.SubscriptionLevel;
            }
            if (userAccount.Signature == string.Empty)
            {
                return SubscriptionLevel.Unknown;
            }

            IAsymmetricPublicKey publicKey = await New<ILicenseAuthority>().PublicKeyAsync();
            if (new Verifier(publicKey).Verify(Convert.FromBase64String(userAccount.Signature), SignedFields(userAccount)))
            {
                return userAccount.SubscriptionLevel;
            }

            return SubscriptionLevel.Unknown;
        }

        private static string[] SignedFields(UserAccount ua)
        {
            string email = ua.UserName;
            string level = ua.SubscriptionLevel.ToString();
            string expiration = ua.LevelExpiration.ToString("u");
            return new string[] { email, level, expiration, };
        }
    }
}
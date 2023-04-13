using AxCrypt.Api.Model;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.Extensions;
using AxCrypt.Core.Session;
using AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Runtime
{
    public class LicenseValidation
    {
        public virtual async Task<string> SignAsync(UserAccount userAccount)
        {
            if (userAccount == null)
            {
                throw new ArgumentNullException(nameof(userAccount));
            }

            if (userAccount.SubscriptionLevel <= SubscriptionLevel.Free)
            {
                return string.Empty;
            }

            IAsymmetricPrivateKey privateKey = await New<ILicenseAuthority>().PrivateKeyAsync();
            byte[] signature = new Signer(privateKey).Sign(SignedFields(userAccount));

            return Convert.ToBase64String(signature);
        }

        public virtual async Task<SubscriptionLevel> ValidateLevelAsync(UserAccount userAccount)
        {
            if (userAccount == null)
            {
                throw new ArgumentNullException(nameof(userAccount));
            }

            if (userAccount.SubscriptionLevel <= SubscriptionLevel.Free)
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

            using (KnownPublicKeys knownKeys = New<KnownPublicKeys>())
            {
                IAsymmetricPublicKey? maybeUpdatedpublicKey = (await knownKeys.GetAsync(EmailAddress.Parse(New<UserSettings>().LicenseAuthorityEmail), New<KnownIdentities>().DefaultEncryptionIdentity))?.PublicKey;
                if (maybeUpdatedpublicKey != null && new Verifier(maybeUpdatedpublicKey).Verify(Convert.FromBase64String(userAccount.Signature), SignedFields(userAccount)))
                {
                    return userAccount.SubscriptionLevel;
                }
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

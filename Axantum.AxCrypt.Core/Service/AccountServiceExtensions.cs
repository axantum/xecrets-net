using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.Service
{
    public static class AccountServiceExtensions
    {
        public static async Task<bool> IsIdentityValidAsync(this IAccountService service)
        {
            if (service.Identity == LogOnIdentity.Empty)
            {
                return false;
            }

            UserAccount account = await service.AccountAsync().Free();
            if (!account.AccountKeys.Select(k => k.ToUserKeyPair(service.Identity.Passphrase)).Any((ukp) => ukp != null))
            {
                return false;
            }
            return true;
        }

        public static async Task<bool> IsAccountSourceLocalAsync(this IAccountService service)
        {
            UserAccount userAccount = await service.AccountAsync();

            return userAccount.AccountSource == AccountSource.Local;
        }
    }
}
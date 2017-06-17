using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Runtime;

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
    }
}
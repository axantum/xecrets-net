using System;
using System.Threading.Tasks;
using AxCrypt.Api.Model;
using AxCrypt.Common;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Service;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Runtime
{
    public static class UserAccountInfo
    {
        private static UserAccount _UserAccount { get; set; } = new UserAccount();

        public static UserAccount UserAccount
        {
            get
            {
                if (_UserAccount == new UserAccount())
                {
                    throw new InvalidOperationException("Use after intilaize!");
                }

                return _UserAccount;
            }
        }

        private static async Task InternalGetAsync(LogOnIdentity identity)
        {
            IAccountService service = New<LogOnIdentity, IAccountService>(identity);
            _UserAccount = await service.AccountAsync().Free();
        }

        public static async Task LoadAsync(LogOnIdentity identity)
        {
            await InternalGetAsync(identity);
        }
    }
}
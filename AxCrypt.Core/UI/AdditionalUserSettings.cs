using AxCrypt.Core.Crypto;
using AxCrypt.Core.Service.Secrets;
using System.Threading.Tasks;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.UI
{
    public class AdditionalUserSettings
    {
        public AdditionalUserSettings(LogOnIdentity identity)
        {
            Identity = identity;
        }

        public LogOnIdentity Identity
        {
            get; private set;
        }

        private string UserEmail
        {
            get
            {
                return Identity?.UserEmail.Address ?? string.Empty;
            }
        }

        public long FreeUserSecretsCount
        {
            get
            {
                return Task.Run(async () => await New<LogOnIdentity, ISecretsService>(Identity).GetFreeUserSecretsCount(UserEmail)).Result;
            }
        }

        public bool UpdateFreeUserSecretsCount()
        {
            return Task.Run(async () => await New<LogOnIdentity, ISecretsService>(Identity).InsertFreeUserSecretsAsync(UserEmail)).Result;
        }
    }
}
using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Service;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Runtime
{
    public class PremiumInfo
    {
        public PremiumStatus PremiumStatus { get; }

        public int DaysLeft { get; }

        public static async Task<PremiumInfo> CreateAsync(LogOnIdentity identity)
        {
            if (identity == LogOnIdentity.Empty)
            {
                return LastKnown();
            }

            IAccountService service = New<LogOnIdentity, IAccountService>(identity);

            SubscriptionLevel level = (await service.AccountAsync().Free()).SubscriptionLevel;
            switch (level)
            {
                case SubscriptionLevel.Unknown:
                case SubscriptionLevel.Free:
                    return await NoPremiumOrCanTryAsync(service);

                case SubscriptionLevel.Premium:
                    return new PremiumInfo(PremiumStatus.HasPremium, await GetDaysLeft(service));

                case SubscriptionLevel.DefinedByServer:
                case SubscriptionLevel.Undisclosed:
                default:
                    return new PremiumInfo(PremiumStatus.NoPremium, 0);
            }
        }

        private PremiumInfo(PremiumStatus premiumStatus, int daysLeft)
        {
            PremiumStatus = premiumStatus;
            DaysLeft = daysLeft;
        }

        private static PremiumInfo LastKnown()
        {
            UserSettings settings = New<UserSettings>();

            return new PremiumInfo(settings.LastKnownPremiumStatus, settings.LastKnownPremiumDaysLeft);
        }

        private static async Task<int> GetDaysLeft(IAccountService service)
        {
            DateTime expiration = (await service.AccountAsync().Free()).LevelExpiration;
            if (expiration == DateTime.MaxValue || expiration == DateTime.MinValue)
            {
                return int.MaxValue;
            }

            DateTime utcNow = New<INow>().Utc;
            if (expiration < utcNow)
            {
                return 0;
            }

            double totalDays = (expiration - utcNow).TotalDays;

            return totalDays > int.MaxValue ? int.MaxValue : (int)totalDays;
        }

        private static async Task<PremiumInfo> NoPremiumOrCanTryAsync(IAccountService service)
        {
            if (New<AxCryptOnlineState>().IsOffline)
            {
                return new PremiumInfo(PremiumStatus.OfflineNoPremium, 0);
            }

            if (!(await service.AccountAsync().Free()).Offers.HasFlag(Offers.AxCryptTrial))
            {
                return new PremiumInfo(PremiumStatus.CanTryPremium, 0);
            }

            return new PremiumInfo(PremiumStatus.NoPremium, 0);
        }
    }
}
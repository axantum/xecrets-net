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

        public bool IsLastKnown { get; }

        public static async Task<PremiumInfo> CreateAsync(LogOnIdentity identity)
        {
            if (identity == LogOnIdentity.Empty)
            {
                return LastKnown();
            }

            PremiumInfo pi = await GetPremiumInfo(identity);
            UserSettings settings = New<UserSettings>();
            settings.LastKnownPremiumStatus = pi.PremiumStatus;
            settings.LastKnownPremiumDaysLeft = pi.DaysLeft;

            return pi;
        }

        private static async Task<PremiumInfo> GetPremiumInfo(LogOnIdentity identity)
        {
            IAccountService service = New<LogOnIdentity, IAccountService>(identity);

            SubscriptionLevel level = (await service.AccountAsync().Free()).SubscriptionLevel;
            switch (level)
            {
                case SubscriptionLevel.Unknown:
                case SubscriptionLevel.Free:
                    return await NoPremiumOrCanTryAsync(service);

                case SubscriptionLevel.Premium:
                    return new PremiumInfo(PremiumStatus.HasPremium, await GetDaysLeft(service), true);

                case SubscriptionLevel.DefinedByServer:
                case SubscriptionLevel.Undisclosed:
                default:
                    return new PremiumInfo(PremiumStatus.NoPremium, 0, true);
            }
        }

        private PremiumInfo(PremiumStatus premiumStatus, int daysLeft, bool isLastKnown)
        {
            PremiumStatus = premiumStatus;
            DaysLeft = daysLeft;
            IsLastKnown = isLastKnown;
        }

        private static PremiumInfo LastKnown()
        {
            UserSettings settings = New<UserSettings>();
            switch (settings.LastKnownPremiumStatus)
            {
                case PremiumStatus.NoPremium:
                case PremiumStatus.OfflineNoPremium:
                    return new PremiumInfo(settings.LastKnownPremiumStatus, 0, false);

                case PremiumStatus.Unknown:
                case PremiumStatus.HasPremium:
                    return new PremiumInfo(PremiumStatus.HasPremium, int.MaxValue, false);

                case PremiumStatus.CanTryPremium:
                    return new PremiumInfo(PremiumStatus.CanTryPremium, 0, false);

                default:
                    break;
            }

            PremiumInfo pi = new PremiumInfo(settings.LastKnownPremiumStatus, settings.LastKnownPremiumDaysLeft, false);
            return pi;
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
                return new PremiumInfo(PremiumStatus.OfflineNoPremium, 0, true);
            }

            if (!(await service.AccountAsync().Free()).Offers.HasFlag(Offers.AxCryptTrial))
            {
                return new PremiumInfo(PremiumStatus.CanTryPremium, 0, true);
            }

            return new PremiumInfo(PremiumStatus.NoPremium, 0, true);
        }
    }
}
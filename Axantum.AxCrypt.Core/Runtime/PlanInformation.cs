using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Service;
using System;
using System.Threading.Tasks;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Runtime
{
    public class PlanInformation : IEquatable<PlanInformation>
    {
        public static readonly PlanInformation Empty = new PlanInformation(PlanState.Unknown, -1);

        public PlanState PlanState { get; }

        public int DaysLeft { get; }

        public static async Task<PlanInformation> CreateAsync(LogOnIdentity identity)
        {
            if (identity == LogOnIdentity.Empty)
            {
                return new PlanInformation(PlanState.NoPremium, 0);
            }

            PlanInformation pi = await GetPlanInformationAsync(identity);
            return pi;
        }

        private static async Task<PlanInformation> GetPlanInformationAsync(LogOnIdentity identity)
        {
            IAccountService service = New<LogOnIdentity, IAccountService>(identity);

            SubscriptionLevel level = await (await service.AccountAsync().Free()).ValidatedLevelAsync();
            switch (level)
            {
                case SubscriptionLevel.Unknown:
                case SubscriptionLevel.Free:
                    return await NoPremiumOrCanTryAsync(service);

                case SubscriptionLevel.Business:
                case SubscriptionLevel.Premium:
                    return new PlanInformation(PlanState.HasPremium, await GetDaysLeft(service));

                case SubscriptionLevel.DefinedByServer:
                case SubscriptionLevel.Undisclosed:
                default:
                    return new PlanInformation(PlanState.NoPremium, 0);
            }
        }

        private PlanInformation(PlanState planStatus, int daysLeft)
        {
            PlanState = planStatus;
            DaysLeft = daysLeft;
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

        private static async Task<PlanInformation> NoPremiumOrCanTryAsync(IAccountService service)
        {
            if (New<AxCryptOnlineState>().IsOffline)
            {
                return new PlanInformation(PlanState.OfflineNoPremium, 0);
            }

            if (!(await service.AccountAsync().Free()).Offers.HasFlag(Offers.AxCryptTrial))
            {
                return new PlanInformation(PlanState.CanTryPremium, 0);
            }

            return new PlanInformation(PlanState.NoPremium, 0);
        }

        public bool Equals(PlanInformation other)
        {
            if ((object)other == null)
            {
                return false;
            }

            return PlanState == other.PlanState && DaysLeft == other.DaysLeft;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || typeof(PlanInformation) != obj.GetType())
            {
                return false;
            }
            PlanInformation other = (PlanInformation)obj;

            return Equals(other);
        }

        public override int GetHashCode()
        {
            return PlanState.GetHashCode() ^ DaysLeft.GetHashCode();
        }

        public static bool operator ==(PlanInformation left, PlanInformation right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }
            if ((object)left == null)
            {
                return false;
            }
            return left.Equals(right);
        }

        public static bool operator !=(PlanInformation left, PlanInformation right)
        {
            return !(left == right);
        }
    }
}
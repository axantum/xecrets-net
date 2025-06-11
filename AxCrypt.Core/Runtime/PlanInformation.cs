﻿using AxCrypt.Abstractions;
using AxCrypt.Api.Model;
using AxCrypt.Common;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Extensions;
using AxCrypt.Core.Service;
using System;
using System.Threading.Tasks;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Runtime
{
    public class PlanInformation : IEquatable<PlanInformation>
    {
        public static readonly PlanInformation Empty = new PlanInformation(PlanState.Unknown, -1, false, false, false);

        public PlanState PlanState { get; }

        public int DaysLeft { get; }

        public bool CanTryPremiumSubscription { get; } = false;

        public bool SubscribedFromAppStore { get; } = false;

        public bool BusinessAdmin { get; } = false;

        public static async Task<PlanInformation> CreateAsync(LogOnIdentity identity)
        {
            if (identity == LogOnIdentity.Empty)
            {
                return new PlanInformation(PlanState.NoPremium, 0, false, false, false);
            }
            if (New<AxCryptOnlineState>().IsOffline)
            {
                return new PlanInformation(PlanState.OfflineNoPremium, 0, false, false, false);
            }

            await UserAccountInfo.LoadAsync(identity).Free();

            UserAccount userAccount = UserAccountInfo.UserAccount;
            return await InternalCreatePlanInformationAsync(userAccount);
        }

        public static async Task<PlanInformation> InternalCreatePlanInformationAsync(UserAccount userAccount)
        {
            SubscriptionLevel level = await userAccount.ValidatedLevelAsync();
            switch (level)
            {
                case SubscriptionLevel.Unknown:
                case SubscriptionLevel.Free:
                    return NoPremiumOrCanTryAsync(userAccount);

                case SubscriptionLevel.Business:
                    return new PlanInformation(PlanState.HasBusiness, GetDaysLeft(userAccount.LevelExpiration), false, userAccount.ActiveSubscriptionFromAppStore, userAccount.BusinessAdmin);

                case SubscriptionLevel.Premium:
                    return new PlanInformation(PlanState.HasPremium, GetDaysLeft(userAccount.LevelExpiration), false, userAccount.ActiveSubscriptionFromAppStore, userAccount.BusinessAdmin);

                case SubscriptionLevel.PasswordManager:
                    return new PlanInformation(PlanState.HasPasswordManager, GetDaysLeft(userAccount.LevelExpiration), false, userAccount.ActiveSubscriptionFromAppStore, userAccount.BusinessAdmin);

                case SubscriptionLevel.DefinedByServer:
                case SubscriptionLevel.Undisclosed:
                default:
                    return new PlanInformation(PlanState.NoPremium, 0, false, false, false);
            }
        }

        private PlanInformation(PlanState planStatus, int daysLeft, bool canTryPremiumSubscription, bool subscribedFromAppStore, bool businessAdmin)
        {
            PlanState = planStatus;
            DaysLeft = daysLeft;
            CanTryPremiumSubscription = canTryPremiumSubscription;
            SubscribedFromAppStore = subscribedFromAppStore;
            BusinessAdmin = businessAdmin;
        }

        private static int GetDaysLeft(DateTime levelExpiration)
        {
            DateTime expiration = levelExpiration;
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

        private static PlanInformation NoPremiumOrCanTryAsync(UserAccount userAccount)
        {
            Offers offers = userAccount.Offers;

            if (!offers.HasFlag(Offers.AxCryptTrial))
            {
                return new PlanInformation(PlanState.CanTryPremium, 0, userAccount.CanTryAppStorePremiumTrial, false, userAccount.BusinessAdmin);
            }

            return new PlanInformation(PlanState.NoPremium, 0, userAccount.CanTryAppStorePremiumTrial, false, userAccount.BusinessAdmin);
        }

        public bool Equals(PlanInformation? other)
        {
            if ((object?)other == null)
            {
                return false;
            }

            return PlanState == other.PlanState && DaysLeft == other.DaysLeft;
        }

        public override bool Equals(object? obj)
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

        public static bool operator ==(PlanInformation? left, PlanInformation? right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }
            if ((object?)left == null)
            {
                return false;
            }
            return left.Equals(right);
        }

        public static bool operator !=(PlanInformation? left, PlanInformation? right)
        {
            return !(left == right);
        }
    }
}

using System;
using System.Collections.Generic;

namespace AxCrypt.Api.Model
{
    public enum SubsCancelnRsnType
    {
        None = 0,
        Usage,
        Pricing,
        Technical
    }

    public class CancellationTypeMapper
    {

        private static readonly IDictionary<SubsCancelnRsnOptn, SubsCancelnRsnType> _CancelRsnTypeMapping = new Dictionary<SubsCancelnRsnOptn, SubsCancelnRsnType>
        {
            { SubsCancelnRsnOptn.IsChangePlan, SubsCancelnRsnType.Usage },
            { SubsCancelnRsnOptn.IsNotUsingProduct, SubsCancelnRsnType.Usage },
            { SubsCancelnRsnOptn.IsNeedProductForProject, SubsCancelnRsnType.Usage },
            { SubsCancelnRsnOptn.FoundBetterAlternative, SubsCancelnRsnType.Usage },
            { SubsCancelnRsnOptn.UsageOther, SubsCancelnRsnType.Usage },

            { SubsCancelnRsnOptn.IsTooExpensive, SubsCancelnRsnType.Pricing },
            { SubsCancelnRsnOptn.IsTrialEnded, SubsCancelnRsnType.Pricing },
            { SubsCancelnRsnOptn.FoundBetterDeal, SubsCancelnRsnType.Pricing },
            { SubsCancelnRsnOptn.IsDontLikeRecurringFlow, SubsCancelnRsnType.Pricing },
            { SubsCancelnRsnOptn.PricingOther, SubsCancelnRsnType.Pricing },

            { SubsCancelnRsnOptn.IsDontWorkExpected, SubsCancelnRsnType.Technical },
            { SubsCancelnRsnOptn.MissingFeature, SubsCancelnRsnType.Technical },
            { SubsCancelnRsnOptn.IsMoreTechnicalIssue, SubsCancelnRsnType.Technical },
            { SubsCancelnRsnOptn.IsProductFeatureComplicated, SubsCancelnRsnType.Technical },
            { SubsCancelnRsnOptn.SupportNotHelpful, SubsCancelnRsnType.Technical },
            { SubsCancelnRsnOptn.TechnicalOther, SubsCancelnRsnType.Technical }
        };

        public static SubsCancelnRsnType GetCancellationType(string reason)
        {
            SubsCancelnRsnType cancellationType = SubsCancelnRsnType.None;
            if (string.IsNullOrEmpty(reason))
            {
                return cancellationType;
            }

            SubsCancelnRsnOptn reasonoption = (SubsCancelnRsnOptn)Enum.Parse(typeof(SubsCancelnRsnOptn), reason);
            if (_CancelRsnTypeMapping.TryGetValue(reasonoption, out cancellationType))
            {
                return cancellationType;
            }

            return cancellationType;
        }
    }
}
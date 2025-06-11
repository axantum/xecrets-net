using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxCrypt.Api.Model
{
    public class SubsCancellationApiModel : BaseApiModel
    {
        [JsonPropertyName("useremail")]
        public string? UserEmail { get; set; }

        [JsonPropertyName("subslvl")]
        public string? SubscriptionLevel { get; set; }

        [JsonPropertyName("cancelnoptns")]
        public string? CancelationReason { get; set; }

        public IEnumerable<string> CancelationReasonNames
        {
            get
            {
                if (string.IsNullOrEmpty(CancelationReason))
                {
                    return new List<string>();
                }

                IEnumerable<int> cancelationReasons = CancelationReason.Split(',').Select(cr => int.Parse(cr));
                return cancelationReasons.Select(cr => Enum.GetName(typeof(SubsCancelnRsnOptn), cr))!;
            }
        }

        [JsonPropertyName("comment")]
        public string? Comment { get; set; }

        [JsonPropertyName("userrecommendation")]
        public string? UserRecommendation { get; set; }

        [JsonPropertyName("paymentprovider")]
        public string? PaymentProvider { get; set; }

        [JsonPropertyName("cancelnoptoutrsn")]
        public string? CancelnOptOutRsn { get; set; }
    }
}

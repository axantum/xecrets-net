using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxCrypt.Api.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SubsCancellationApiModel : BaseApiModel
    {
        [JsonProperty("useremail")]
        public string UserEmail { get; set; }

        [JsonProperty("subslvl")]
        public string SubscriptionLevel { get; set; }

        [JsonProperty("cancelnoptns")]
        public string CancelationReason { get; set; }

        public IEnumerable<string> CancelationReasonNames
        {
            get
            {
                if (string.IsNullOrEmpty(CancelationReason))
                {
                    return new List<string>();
                }

                IEnumerable<int> cancelationReasons = CancelationReason.Split(',').Select(cr => int.Parse(cr));
                return cancelationReasons.Select(cr => Enum.GetName(typeof(SubsCancelnRsnOptn), cr));
            }
        }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("userrecommendation")]
        public string UserRecommendation { get; set; }

        [JsonProperty("paymentprovider")]
        public string PaymentProvider { get; set; }

        [JsonProperty("cancelnoptoutrsn")]
        public string CancelnOptOutRsn { get; set; }
    }
}
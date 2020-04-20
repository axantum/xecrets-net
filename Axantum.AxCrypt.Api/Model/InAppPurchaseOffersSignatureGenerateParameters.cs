using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Api.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class InAppPurchaseOffersSignatureGenerateParameters
    {
        public InAppPurchaseOffersSignatureGenerateParameters()
        {

        }

        [JsonProperty("app_bundle_id")]
        public string AppBundleID { get; set; }

        [JsonProperty("app_bundle_id")]
        public string ProductIdentifier { get; set; }

        [JsonProperty("offer_identifier")]
        public string OfferIdentifier { get; set; }

        [JsonProperty("application_username")]
        public string ApplicationUsername { get; set; }

        [JsonProperty("key_identifier")]
        public string KeyIdentifier { get; set; }

        [JsonProperty("nonce")]
        public string Nonce { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }
    }
}
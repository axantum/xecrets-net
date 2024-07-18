using Newtonsoft.Json;

namespace AxCrypt.Api.Model.UTM
{
    public class UTMTagPurchaseActivityApiModel : UTMTagUserActivityApiModel
    {
        [JsonProperty("subslevel")]
        public string SubsLevel { get; set; }

        [JsonProperty("substype")]
        public string SubsType { get; set; }

        [JsonProperty("subsid")]
        public string SubsId { get; set; }

        [JsonProperty("transid")]
        public string TransId { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }
    }
}

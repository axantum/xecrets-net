using System.Text.Json.Serialization;

namespace AxCrypt.Api.Model.UTM
{
    public class UTMTagPurchaseActivityApiModel : UTMTagUserActivityApiModel
    {
        [JsonPropertyName("subslevel")]
        public string? SubsLevel { get; set; }

        [JsonPropertyName("substype")]
        public string? SubsType { get; set; }

        [JsonPropertyName("subsid")]
        public string? SubsId { get; set; }

        [JsonPropertyName("transid")]
        public string? TransId { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
    }
}

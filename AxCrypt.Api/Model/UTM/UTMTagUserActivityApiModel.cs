using System.Text.Json.Serialization;

namespace AxCrypt.Api.Model.UTM
{
    public class UTMTagUserActivityApiModel : UTMTagActivityApiModel
    {
        [JsonPropertyName("useremail")]
        public string UserEmail { get; set; }
    }
}

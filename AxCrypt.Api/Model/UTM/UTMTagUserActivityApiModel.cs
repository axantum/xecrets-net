using Newtonsoft.Json;

namespace AxCrypt.Api.Model.UTM
{
    public class UTMTagUserActivityApiModel : UTMTagActivityApiModel
    {
        [JsonProperty("useremail")]
        public string UserEmail { get; set; }
    }
}

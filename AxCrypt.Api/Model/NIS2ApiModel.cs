using Newtonsoft.Json;

namespace AxCrypt.Api.Model
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class NIS2ApiModel : BaseApiModel
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("fullname")]
        public string FullName { get; set; }

        [JsonProperty("workemail")]
        public string WorkEmail { get; set; }

        [JsonProperty("companyname")]
        public string CompanyName { get; set; }

        [JsonProperty("companysize")]
        public string CompanySize { get; set; }

        [JsonProperty("jobtitle")]
        public string JobTitle { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("grecaptcharesponse")]
        public string GreCaptchaResponse { get; set; }
    }
}
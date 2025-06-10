using Newtonsoft.Json;

namespace AxCrypt.Api.Model.User
{
    [JsonObject(MemberSerialization.OptIn)]
    public class TwoFactorAuthApiModel : BaseApiModel
    {
        public TwoFactorAuthApiModel()
        { }

        [JsonProperty("userEmail")]
        public string UserEmail { get; set; }

        [JsonProperty("uniqueKey")]
        public string UniqueKey { get; set; }

        [JsonProperty("backupCodeSalt")]
        public string BackupCodeSalt { get; set; }

        [JsonProperty("backupCodeHash")]
        public string BackupCodeHash { get; set; }
    }
}
using System.Text.Json.Serialization;

namespace AxCrypt.Api.Model.User
{
    public class TwoFactorAuthApiModel : BaseApiModel
    {
        public TwoFactorAuthApiModel()
        { }

        [JsonPropertyName("userEmail")]
        public string? UserEmail { get; set; }

        [JsonPropertyName("uniqueKey")]
        public string? UniqueKey { get; set; }

        [JsonPropertyName("backupCodeSalt")]
        public string? BackupCodeSalt { get; set; }

        [JsonPropertyName("backupCodeHash")]
        public string? BackupCodeHash { get; set; }
    }
}

using Newtonsoft.Json;
using System;

namespace AxCrypt.Api.Model.User
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UserApiModel : BaseApiModel
    {
        public static UserApiModel Empty = new UserApiModel(0, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, DateTime.MinValue, DateTime.MinValue);

        public UserApiModel()
        {
        }

        public UserApiModel(long id, string userEmail, string role, string passwordSalt, string passwordHash, string activationCode, DateTime createdUtc, DateTime updatedUtc)
        {
            Id = id;
            UserEmail = userEmail;
            Role = role;
            PasswordSalt = passwordSalt;
            PasswordHash = passwordHash;
            ActivationCode = activationCode;
            CreatedUtc = createdUtc;
            UpdatedUtc = updatedUtc;
        }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("userEmail")]
        public string UserEmail { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("passwordSalt")]
        public string PasswordSalt { get; set; }

        [JsonProperty("passwordHash")]
        public string PasswordHash { get; set; }

        [JsonProperty("providerUserKey")]
        public string ProviderUserKey { get; set; }

        [JsonProperty("activationCode")]
        public string ActivationCode { get; set; }

        [JsonProperty("approvedTime")]
        public DateTime ApprovedTime { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }

        [JsonProperty("lang")]
        public int Lang { get; set; }

        [JsonProperty("origin")]
        public int Origin { get; set; }

        [JsonProperty("lastPwdResetTime")]
        public DateTime LastPwdResetTime { get; set; }

        [JsonProperty("lastPwdChangedTime")]
        public DateTime LastPwdChangedTime { get; set; }

        [JsonProperty("lastLogonTime")]
        public DateTime LastLogOnTime { get; set; }

        [JsonProperty("lastLockedOutTime")]
        public DateTime LastLockedOutTime { get; set; }

        [JsonProperty("failedPwdAtmptCount")]
        public int FailedPwdAtmptCount { get; set; }

        [JsonProperty("failedPwdAtmptWindStart")]
        public int FailedPwdAtmptWindStart { get; set; }

        [JsonProperty("is2FAEnabled")]
        public bool Is2FAEnabled { get; set; }
    }
}
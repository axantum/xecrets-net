using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxCrypt.Api.Model.User
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UserApiModel
    {
        public static UserApiModel Empty = new UserApiModel(0, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, DateTime.MinValue, DateTime.MinValue);

        public UserApiModel(long id, string userEmail, string role, string passwordSalt, string passwordHash, string activationCode, DateTime createdUtc, DateTime updatedUtc)
        {
            Id = id;
            UserEmail = userEmail;
            Role = role;
            PasswordSalt = passwordSalt;
            PasswordHash = passwordHash;
            ActivationCode = activationCode;
            Created = createdUtc;
            Updated = updatedUtc;
        }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("user_email")]
        public string UserEmail { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("password_salt")]
        public string PasswordSalt { get; set; }

        [JsonProperty("password_hash")]
        public string PasswordHash { get; set; }

        [JsonProperty("provider_userkey")]
        public string ProviderUserKey { get; set; }

        [JsonProperty("activation_code")]
        public string ActivationCode { get; set; }

        [JsonProperty("approved_time")]
        public DateTime ApprovedTime { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }

        [JsonProperty("lang")]
        public int Lang { get; set; }

        [JsonProperty("origin")]
        public int Origin { get; set; }

        [JsonProperty("last_pwdreset_time")]
        public DateTime LastPwdResetTime { get; set; }

        [JsonProperty("last_pwdchanged_time")]
        public DateTime LastPwdChangedTime { get; set; }

        [JsonProperty("last_logon_time")]
        public DateTime LastLogOnTime { get; set; }

        [JsonProperty("last_lockedout_time")]
        public DateTime LastLockedOutTime { get; set; }

        [JsonProperty("failed_pwdatmpt_count")]
        public int FailedPwdAtmptCount { get; set; }

        [JsonProperty("failed_pwdatmpt_wind_start")]
        public int FailedPwdAtmptWindStart { get; set; }

        [JsonProperty("created_utc")]
        public DateTime Created { get; set; }

        [JsonProperty("updated_utc")]
        public DateTime Updated { get; set; }

        [JsonProperty("deleted_utc")]
        public DateTime? Deleted { get; set; }
    }
}
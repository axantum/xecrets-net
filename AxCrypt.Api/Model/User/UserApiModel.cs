using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace AxCrypt.Api.Model.User
{
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

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("user_email")]
        public string UserEmail { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("password_salt")]
        public string PasswordSalt { get; set; }

        [JsonPropertyName("password_hash")]
        public string PasswordHash { get; set; }

        [JsonPropertyName("provider_userkey")]
        public string? ProviderUserKey { get; set; }

        [JsonPropertyName("activation_code")]
        public string ActivationCode { get; set; }

        [JsonPropertyName("approved_time")]
        public DateTime ApprovedTime { get; set; }

        [JsonPropertyName("locale")]
        public string? Locale { get; set; }

        [JsonPropertyName("lang")]
        public int Lang { get; set; }

        [JsonPropertyName("origin")]
        public int Origin { get; set; }

        [JsonPropertyName("last_pwdreset_time")]
        public DateTime LastPwdResetTime { get; set; }

        [JsonPropertyName("last_pwdchanged_time")]
        public DateTime LastPwdChangedTime { get; set; }

        [JsonPropertyName("last_logon_time")]
        public DateTime LastLogOnTime { get; set; }

        [JsonPropertyName("last_lockedout_time")]
        public DateTime LastLockedOutTime { get; set; }

        [JsonPropertyName("failed_pwdatmpt_count")]
        public int FailedPwdAtmptCount { get; set; }

        [JsonPropertyName("failed_pwdatmpt_wind_start")]
        public int FailedPwdAtmptWindStart { get; set; }

        [JsonPropertyName("created_utc")]
        public DateTime Created { get; set; }

        [JsonPropertyName("updated_utc")]
        public DateTime Updated { get; set; }

        [JsonPropertyName("deleted_utc")]
        public DateTime? Deleted { get; set; }
    }
}

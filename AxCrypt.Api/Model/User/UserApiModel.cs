using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace AxCrypt.Api.Model.User
{
    public class UserApiModel : BaseApiModel
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
            CreatedUtc = createdUtc;
            UpdatedUtc = updatedUtc;
        }

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("userEmail")]
        public string UserEmail { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("passwordSalt")]
        public string PasswordSalt { get; set; }

        [JsonPropertyName("passwordHash")]
        public string PasswordHash { get; set; }

        [JsonPropertyName("providerUserKey")]
        public string? ProviderUserKey { get; set; }

        [JsonPropertyName("activationCode")]
        public string ActivationCode { get; set; }

        [JsonPropertyName("approvedTime")]
        public DateTime ApprovedTime { get; set; }

        [JsonPropertyName("locale")]
        public string? Locale { get; set; }

        [JsonPropertyName("lang")]
        public int Lang { get; set; }

        [JsonPropertyName("origin")]
        public int Origin { get; set; }

        [JsonPropertyName("lastPwdResetTime")]
        public DateTime LastPwdResetTime { get; set; }

        [JsonPropertyName("lastPwdChangedTime")]
        public DateTime LastPwdChangedTime { get; set; }

        [JsonPropertyName("lastLogonTime")]
        public DateTime LastLogOnTime { get; set; }

        [JsonPropertyName("lastLockedOutTime")]
        public DateTime LastLockedOutTime { get; set; }

        [JsonPropertyName("failedPwdAtmptCount")]
        public int FailedPwdAtmptCount { get; set; }

        [JsonPropertyName("failedPwdAtmptWindStart")]
        public int FailedPwdAtmptWindStart { get; set; }
    }
}

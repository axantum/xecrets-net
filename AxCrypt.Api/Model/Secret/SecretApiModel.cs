using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxCrypt.Api.Model.Secret
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SecretApiModel : BaseApiModel
    {
        public static SecretApiModel Empty = new SecretApiModel(0, string.Empty, string.Empty, string.Empty, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);

        public SecretApiModel(long id, string type, string userEmail, string secretBody, DateTime createdUtc, DateTime updatedUtc, DateTime? deletedUtc)
        {
            Id = id;
            Type = type;
            UserEmail = userEmail;
            SecretBody = secretBody;
            CreatedUtc = createdUtc;
            UpdatedUtc = updatedUtc;
            DeletedUtc = deletedUtc;
        }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("userEmail")]
        public string UserEmail { get; set; } = string.Empty;

        [JsonProperty("secretBody")]
        public string SecretBody { get; set; } = string.Empty;
    }
}
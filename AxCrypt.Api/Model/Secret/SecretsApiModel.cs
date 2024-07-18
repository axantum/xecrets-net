using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxCrypt.Api.Model.Secret
{
    public class SecretsApiModel : BaseApiModel
    {
        public static SecretsApiModel Empty = new SecretsApiModel(0, string.Empty, "", DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, new List<ShareSecretApiModel>());

        public SecretsApiModel(long id, string userEmail, string secretBody, DateTime createdUtc, DateTime updatedUtc, DateTime? deletedUtc, IEnumerable<ShareSecretApiModel> shareSecret)
        {
            Id = id;
            UserEmail = userEmail;
            SecretBody = secretBody;
            ShareSecret = shareSecret;
            CreatedUtc = createdUtc;
            UpdatedUtc = updatedUtc;
            DeletedUtc = deletedUtc;
        }
        
        [JsonPropertyName("id")]
        public long Id { get; set; } = 0;

        [JsonPropertyName("userEmail")]
        public string UserEmail { get; set; } = string.Empty;

        [JsonPropertyName("secretBody")]
        public string SecretBody { get; set; }

        [JsonPropertyName("shareSecret")]
        public IEnumerable<ShareSecretApiModel> ShareSecret { get; set; } = new List<ShareSecretApiModel>();
    }
}

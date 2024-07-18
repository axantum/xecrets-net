using AxCrypt.Core.Secrets;
using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxCrypt.Api.Model.Secret
{
    public class ShareSecretApiModel : BaseApiModel
    {
        public static ShareSecretApiModel Empty = new ShareSecretApiModel(0, string.Empty, Guid.Empty, string.Empty, new List<SharedUserApiModel>(), DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);

        public ShareSecretApiModel(long id, string userEmail, Guid secretId, string encryptedSecret, IEnumerable<SharedUserApiModel> sharedWith, DateTime createdUtc, DateTime updatedUtc, DateTime? deletedUtc)
        {
            Id = id;
            OwnerEmail = userEmail;
            SecretId = secretId;
            EncryptedSecret = encryptedSecret;
            SharedWith = sharedWith;
            CreatedUtc = createdUtc;
            UpdatedUtc = updatedUtc;
            DeletedUtc = deletedUtc;
        }

        [JsonPropertyName("id")]
        public long Id { get; set; } = 0;

        [JsonPropertyName("secretId")]
        public Guid SecretId { get; set; }

        [JsonPropertyName("ownerEmail")]
        public string OwnerEmail { get; set; } = string.Empty;

        [JsonPropertyName("ownerId")]
        public long OwnerId { get; set; }

        [JsonPropertyName("encryptedSecret")]
        public string EncryptedSecret { get; set; } = string.Empty;

        [JsonPropertyName("sharedWith")]
        public IEnumerable<SharedUserApiModel> SharedWith { get; set; } = new List<SharedUserApiModel>();

    }
}

using AxCrypt.Api.Model.Secret;
using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxCrypt.Api.Model.User
{
    public class MigrateApiModel
    {
        public static MigrateApiModel Empty = new MigrateApiModel(UserApiModel.Empty, new List<SecretApiModel> { SecretApiModel.Empty });

        public MigrateApiModel(UserApiModel user, IList<SecretApiModel> secrets)
        {
            User = user;
            Secrets = secrets;
        }

        [JsonPropertyName("user")]
        public UserApiModel User { get; set; }

        [JsonPropertyName("secrets")]
        public IList<SecretApiModel> Secrets { get; set; }
    }
}

using AxCrypt.Api.Model.Secret;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxCrypt.Api.Model.User
{
    [JsonObject(MemberSerialization.OptIn)]
    public class MigrateApiModel
    {
        public static MigrateApiModel Empty = new MigrateApiModel(UserApiModel.Empty, SecretsApiModel.Empty);

        public MigrateApiModel(UserApiModel user, SecretsApiModel secrets)
        {
            User = user;
            Secrets = secrets;
        }

        [JsonProperty("user")]
        public UserApiModel User { get; }

        [JsonProperty("secrets")]
        public SecretsApiModel Secrets { get; }
    }
}
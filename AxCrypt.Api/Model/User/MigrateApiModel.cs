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
        public static MigrateApiModel Empty = new MigrateApiModel(UserApiModel.Empty, new List<SecretApiModel> { SecretApiModel.Empty });

        public MigrateApiModel(UserApiModel user, IList<SecretApiModel> secrets)
        {
            User = user;
            Secrets = secrets;
        }

        [JsonProperty("user")]
        public UserApiModel User { get; }

        [JsonProperty("secrets")]
        public IList<SecretApiModel> Secrets { get; }
    }
}
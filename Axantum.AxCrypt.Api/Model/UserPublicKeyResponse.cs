using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Api.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UserPublicKeyResponse : CommonResponse
    {
        public UserPublicKeyResponse()
        {
            PublicKey = new UserPublicKey();
        }

        [JsonProperty("userpublic")]
        public UserPublicKey PublicKey { get; private set; }
    }
}
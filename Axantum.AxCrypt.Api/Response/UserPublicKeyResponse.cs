using Axantum.AxCrypt.Api.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Api.Response
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UserPublicKeyResponse : ResponseBase
    {
        public UserPublicKeyResponse()
        {
            PublicKey = new UserPublicKey();
        }

        [JsonProperty("userpublic")]
        public UserPublicKey PublicKey { get; private set; }
    }
}
using Axantum.AxCrypt.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Api.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UserAccounts
    {
        public UserAccounts()
        {
            Accounts = new List<UserAccount>();
        }

        [JsonProperty("accounts")]
        public IList<UserAccount> Accounts { get; private set; }

        public void SerializeTo(StreamWriter writer)
        {
            string value = TypeMap.Resolve.New<IStringSerializer>().Serialize(this);
            writer.Write(value);
        }

        public static UserAccounts DeserializeFrom(StreamReader reader)
        {
            string serialized = reader.ReadToEnd();
            return TypeMap.Resolve.New<IStringSerializer>().Deserialize<UserAccounts>(serialized);
        }
    }
}
using Axantum.AxCrypt.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

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

        public void SetAccount(UserAccount account)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            for (int i = 0; i < Accounts.Count; ++i)
            {
                if (Accounts[i].UserName != account.UserName)
                {
                    continue;
                }
                Accounts[i] = account;
                return;
            }
            Accounts.Add(account);
        }

        public void SerializeTo(TextWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            string value = New<IStringSerializer>().Serialize(this);
            writer.Write(value);
        }

        public static UserAccounts DeserializeFrom(TextReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            string serialized = reader.ReadToEnd();
            return New<IStringSerializer>().Deserialize<UserAccounts>(serialized);
        }
    }
}
using AxCrypt.Abstractions;

using System.Text.Json.Serialization;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Api.Model
{
    public class UserAccounts
    {
        public UserAccounts()
        {
            Accounts = new List<UserAccount>();
        }

        [JsonPropertyName("accounts")]
        public IList<UserAccount> Accounts { get; set; }

        public void SetAccount(UserAccount? account)
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

        public static UserAccounts? DeserializeFrom(TextReader reader)
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

using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Session
{
    /// <summary>
    /// A respository for a single user e-mail. A user has a single active key pair, with both a public
    /// key for encryption and the matching private key for decryption.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class UserAsymmetricKeys
    {
        [JsonConstructor]
        private UserAsymmetricKeys()
        {
            TimeStamp = Resolve.Environment.UtcNow;
        }

        public UserAsymmetricKeys(EmailAddress userEmail, int bits)
        {
            UserEmail = userEmail;
            KeyPair = TypeMap.Resolve.Singleton<IAsymmetricFactory>().CreateKeyPair(bits);
        }

        [JsonProperty("timestamp")]
        public DateTime TimeStamp { get; private set; }

        [JsonProperty("useremail")]
        [JsonConverter(typeof(EmailAddressJsonConverter))]
        public EmailAddress UserEmail { get; private set; }

        [JsonProperty("keypair")]
        public IAsymmetricKeyPair KeyPair { get; private set; }
    }
}
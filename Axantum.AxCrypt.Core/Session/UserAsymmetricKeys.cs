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
    /// key for encryption and the matching private key for decryption. There may also be a list of
    /// previously used private keys, in order to be able to decrypt files encrypted with older key pairs.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class UserAsymmetricKeys
    {
        [JsonConstructor]
        private UserAsymmetricKeys()
        {
            RecalledPrivateKeys = new List<IAsymmetricPrivateKey>();
        }

        public UserAsymmetricKeys(EmailAddress userEmail, int bits)
        {
            UserEmail = userEmail;
            RecalledPrivateKeys = new List<IAsymmetricPrivateKey>();
            KeyPair = Factory.Instance.Singleton<IAsymmetricFactory>().CreateKeyPair(bits);
        }

        [JsonProperty("useremail")]
        [JsonConverter(typeof(EmailAddressJsonConverter))]
        public EmailAddress UserEmail { get; private set; }

        [JsonProperty("recalledprivatekeys")]
        public IList<IAsymmetricPrivateKey> RecalledPrivateKeys { get; private set; }

        [JsonProperty("keypair")]
        public IAsymmetricKeyPair KeyPair { get; private set; }
    }
}
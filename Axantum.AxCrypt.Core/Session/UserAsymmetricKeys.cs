using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Extensions;
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

        public UserAsymmetricKeys(string userEmail, int bits)
        {
            UserEmail = userEmail;
            RecalledPrivateKeys = new List<IAsymmetricPrivateKey>();
            KeyPair = Factory.Instance.Singleton<IAsymmetricFactory>().CreateKeyPair(bits);
        }

        [JsonProperty("useremail")]
        public string UserEmail { get; private set; }

        [JsonProperty("recalledprivatekeys")]
        public IList<IAsymmetricPrivateKey> RecalledPrivateKeys { get; private set; }

        [JsonProperty("keypair")]
        public IAsymmetricKeyPair KeyPair { get; private set; }
    }
}
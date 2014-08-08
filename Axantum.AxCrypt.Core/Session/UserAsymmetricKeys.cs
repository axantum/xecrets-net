using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Newtonsoft.Json;
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
            RecalledPrivateKeys = new List<IAsymmetricKey>();
        }

        public UserAsymmetricKeys(string userEmail, int bits)
        {
            UserEmail = userEmail;
            RecalledPrivateKeys = new List<IAsymmetricKey>();
            KeyPair = AsymmetricKeyPair.Create(bits);
        }

        [JsonProperty("useremail")]
        public string UserEmail { get; private set; }

        [JsonProperty("recalledprivatekeys")]
        private IList<AsymmetricPrivateKey> _serializedRecalledPrivateKeys;

        public IList<IAsymmetricKey> RecalledPrivateKeys
        {
            get
            {
                return _serializedRecalledPrivateKeys.Select(k => (IAsymmetricKey)k).ToList();
            }
            private set
            {
                _serializedRecalledPrivateKeys = value.Select(k => (AsymmetricPrivateKey)k).ToList();
            }
        }

        [JsonProperty("keypair")]
        public AsymmetricKeyPair KeyPair { get; private set; }
    }
}
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxCrypt.Api.Model.Masterkey
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class MasterKeyPairInfo : IEquatable<MasterKeyPairInfo>
    {
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("thumbprint")]
        public string Thumbprint { get; set; }

        [JsonProperty("public")]
        public string PublicKey { get; set; }

        [JsonProperty("private")]
        public IList<PrivateMasterKeyInfo> PrivateKeys { get; set; }

        public bool Equals(MasterKeyPairInfo other)
        {
            if ((object)other == null)
            {
                return false;
            }

            if (Timestamp != other.Timestamp)
            {
                return false;
            }
            if (Thumbprint != other.Thumbprint)
            {
                return false;
            }
            if (PublicKey != other.PublicKey)
            {
                return false;
            }

            return PrivateKeys.SequenceEqual(other.PrivateKeys);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || typeof(MasterKeyPairInfo) != obj.GetType())
            {
                return false;
            }
            MasterKeyPairInfo other = (MasterKeyPairInfo)obj;

            return Equals(other);
        }

        public override int GetHashCode()
        {
            return Timestamp.GetHashCode() ^ Thumbprint.GetHashCode() ^ PublicKey.GetHashCode() ^ PrivateKeys.Aggregate(0, (sum, ak) => sum ^ ak.GetHashCode());
        }

        public static bool operator ==(MasterKeyPairInfo left, MasterKeyPairInfo right)
        {
            if (Object.ReferenceEquals(left, right))
            {
                return true;
            }
            if ((object)left == null)
            {
                return false;
            }
            return left.Equals(right);
        }

        public static bool operator !=(MasterKeyPairInfo left, MasterKeyPairInfo right)
        {
            return !(left == right);
        }
    }
}
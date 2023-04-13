using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxCrypt.Api.Model.Masterkey
{
    public class PrivateMasterKeyInfo : IEquatable<PrivateMasterKeyInfo>
    {
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("key")]
        public string? PrivateKey { get; set; }

        [JsonPropertyName("user")]
        public string? UserEmail { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        public bool Equals(PrivateMasterKeyInfo? other)
        {
            if ((object?)other == null)
            {
                return false;
            }

            if (Timestamp != other.Timestamp)
            {
                return false;
            }
            if (Status != other.Status)
            {
                return false;
            }
            if (UserEmail != other.UserEmail)
            {
                return false;
            }

            if (PrivateKey != other.PrivateKey)
            {
                return false;
            }

            return true;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || typeof(PrivateMasterKeyInfo) != obj.GetType())
            {
                return false;
            }
            PrivateMasterKeyInfo other = (PrivateMasterKeyInfo)obj;

            return Equals(other);
        }

        public override int GetHashCode()
        {
            return Timestamp.GetHashCode() ^ Status!.GetHashCode() ^ UserEmail!.GetHashCode() ^ PrivateKey!.GetHashCode();
        }

        public static bool operator ==(PrivateMasterKeyInfo left, PrivateMasterKeyInfo right)
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

        public static bool operator !=(PrivateMasterKeyInfo left, PrivateMasterKeyInfo right)
        {
            return !(left == right);
        }
    }
}

﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxCrypt.Api.Model.Masterkey
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class PrivateMasterKeyInfo : IEquatable<PrivateMasterKeyInfo>
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("key")]
        public string PrivateKey { get; set; }

        [JsonProperty("user")]
        public string UserEmail { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("disableAccess")]
        public bool DisableAccess { get; set; }

        [JsonProperty("masterPublicKeyId")]
        public long MasterPublicKeyId { get; set; }

        public bool Equals(PrivateMasterKeyInfo other)
        {
            if ((object)other == null)
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

        public override bool Equals(object obj)
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
            return Timestamp.GetHashCode() ^ Status.GetHashCode() ^ UserEmail.GetHashCode() ^ PrivateKey.GetHashCode();
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
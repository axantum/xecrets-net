using Axantum.AxCrypt.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.UI
{
    /// <summary>
    /// A strongly typed representation of an e-mail address.
    /// </summary>
    /// <remarks>Instances of this type are immutable.</remarks>
    [JsonObject(MemberSerialization.OptIn)]
    public class EmailAddress : IEquatable<EmailAddress>
    {
        public static readonly EmailAddress Empty = new EmailAddress();

        [JsonProperty("address")]
        public string Address { get; private set; }

        private EmailAddress()
        {
            Address = String.Empty;
        }

        private EmailAddress(string address)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }

            string parsed;
            if (!TypeMap.Resolve.Singleton<IEmailParser>().TryParse(address, out parsed))
            {
                throw new FormatException("Not recognized as a valid e-mail.");
            }

            Address = parsed;
        }

        public static EmailAddress Parse(string address)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }

            if (address.Length == 0)
            {
                return Empty;
            }

            return new EmailAddress(address);
        }

        public override string ToString()
        {
            return Address;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as EmailAddress);
        }

        public override int GetHashCode()
        {
            return Address.GetHashCode();
        }

        public static bool operator ==(EmailAddress left, EmailAddress right)
        {
            if (Object.ReferenceEquals(left, null))
            {
                return Object.ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(EmailAddress left, EmailAddress right)
        {
            return !(left == right);
        }

        public bool Equals(EmailAddress other)
        {
            if (Object.ReferenceEquals(other, null) || GetType() != other.GetType())
            {
                return false;
            }

            return Address == other.Address;
        }
    }
}
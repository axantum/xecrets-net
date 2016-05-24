using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.UI
{
    /// <summary>
    /// A strongly typed representation of an email address.
    /// </summary>
    /// <remarks>Instances of this type are immutable.</remarks>
    [JsonObject(MemberSerialization.OptIn)]
    public class EmailAddress : IEquatable<EmailAddress>
    {
        public static EmailAddress Empty { get { return new EmailAddress(String.Empty); } }

        [JsonProperty("address")]
        public string Address { get; private set; }

        [JsonConstructor]
        private EmailAddress(string address)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }

            if (address.Length == 0)
            {
                Address = String.Empty;
                return;
            }

            string parsed;
            if (!New<IEmailParser>().TryParse(address, out parsed))
            {
                throw new FormatException("Not recognized as a valid email address.");
            }

            Address = parsed;
        }

        public static bool TryParse(string address, out EmailAddress email)
        {
            email = Empty;

            if (address == null)
            {
                throw new ArgumentNullException("address");
            }

            if (address.Length == 0)
            {
                return true;
            }

            string parsed;
            if (!New<IEmailParser>().TryParse(address, out parsed))
            {
                return false;
            }

            email = new EmailAddress(parsed);
            return true;
        }

        public static EmailAddress Parse(string address)
        {
            EmailAddress email;
            if (TryParse(address, out email))
            {
                return email;
            }

            throw new FormatException("Not recognized as a valid email address.");
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

            return String.Compare(Address, other.Address, StringComparison.OrdinalIgnoreCase) == 0;
        }
    }
}
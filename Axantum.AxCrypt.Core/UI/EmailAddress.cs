using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.UI
{
    /// <summary>
    /// A strongly typed representation of an e-mail address.
    /// </summary>
    /// <remarks>Instances of this type are immutable.</remarks>
    public class EmailAddress : IEquatable<EmailAddress>
    {
        public static readonly EmailAddress Empty = new EmailAddress(String.Empty);

        public string Address { get; private set; }

        public EmailAddress(string address)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }

            Address = address.Trim().ToLowerInvariant();
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
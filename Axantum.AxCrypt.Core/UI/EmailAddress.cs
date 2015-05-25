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
    public class EmailAddress
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
            if (obj == null || obj.GetType() != typeof(EmailAddress))
            {
                return false;
            }

            EmailAddress right = (EmailAddress)obj;
            return right.Address == Address;
        }

        public override int GetHashCode()
        {
            return Address.GetHashCode();
        }

        public static bool operator ==(EmailAddress left, EmailAddress right)
        {
            return Object.Equals(left, right);
        }

        public static bool operator !=(EmailAddress left, EmailAddress right)
        {
            return !(left == right);
        }
    }
}
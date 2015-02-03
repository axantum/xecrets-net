using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.UI
{
    public class EmailAddress
    {
        public static readonly EmailAddress Empty = new EmailAddress(String.Empty);

        public string Address { get; set; }

        public EmailAddress(string address)
        {
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
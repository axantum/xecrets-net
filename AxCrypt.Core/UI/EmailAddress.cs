using AxCrypt.Abstractions.Algorithm;
using AxCrypt.Core.Extensions;

using System.Globalization;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.UI
{
    /// <summary>
    /// A strongly typed representation of an email address.
    /// </summary>
    /// <remarks>Instances of this type are immutable.</remarks>
    public class EmailAddress : IEquatable<EmailAddress>, IComparable<EmailAddress>
    {
        public static EmailAddress Empty { get { return new EmailAddress(string.Empty); } }

        public string Address { get; private set; } = string.Empty;

        private EmailAddress(string address)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            if (address.Length == 0)
            {
                Address = string.Empty;
                return;
            }

            if (!New<IEmailParser>().TryParse(address, out string? parsed))
            {
                throw new FormatException("Not recognized as a valid email address.");
            }

            Address = parsed ?? string.Empty;
        }

        public static bool TryParse(string address, out EmailAddress email)
        {
            email = Empty;

            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            if (address.Length == 0)
            {
                return true;
            }

            if (!New<IEmailParser>().TryParse(address, out string? parsed))
            {
                return false;
            }

            email = new EmailAddress(parsed ?? string.Empty);
            return true;
        }

        public static EmailAddress Parse(string address)
        {
            if (TryParse(address, out EmailAddress email))
            {
                return email;
            }

            throw new FormatException("Not recognized as a valid email address.");
        }

        public static IEnumerable<EmailAddress> Extract(string text)
        {
            return New<IEmailParser>().Extract(text).Select(s => new EmailAddress(s));
        }

        public override string ToString()
        {
            return Address;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as EmailAddress);
        }

        public string Tag
        {
            get
            {
                string canonical = Address.ToUpperInvariant();
                uint id = BitConverter.ToUInt32(New<Sha256>().ComputeHash(System.Text.Encoding.UTF8.GetBytes(canonical)).Reduce(4), 0);
                return id.ToString("x8", CultureInfo.InvariantCulture);
            }
        }

        public override int GetHashCode()
        {
            return Address.GetHashCode();
        }

        public static bool operator ==(EmailAddress? left, EmailAddress? right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(EmailAddress? left, EmailAddress? right)
        {
            return !(left == right);
        }

        public static bool operator <(EmailAddress? left, EmailAddress? right)
        {
            if (left == null || right == null)
            {
                return false;
            }
            return string.Compare(left.Address, right.Address, StringComparison.OrdinalIgnoreCase) < 0;
        }

        public static bool operator >(EmailAddress? left, EmailAddress? right)
        {
            if (left == null || right == null)
            {
                return false;
            }
            return string.Compare(left.Address, right.Address, StringComparison.OrdinalIgnoreCase) > 0;
        }

        public bool Equals(EmailAddress? other)
        {
            if (other is null || GetType() != other.GetType())
            {
                return false;
            }

            return string.Compare(Address, other.Address, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public int CompareTo(EmailAddress? other)
        {
            if (other == null)
            {
                return 1;
            }
            return Address.CompareTo(other.Address);
        }
    }
}

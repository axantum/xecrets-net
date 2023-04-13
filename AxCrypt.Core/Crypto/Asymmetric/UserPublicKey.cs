#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
 *
 * This file is part of AxCrypt.
 *
 * AxCrypt is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * AxCrypt is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AxCrypt.  If not, see <http://www.gnu.org/licenses/>.
 *
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Core.UI;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace AxCrypt.Core.Crypto.Asymmetric
{
    /// <summary>
    /// Holder of the public key for a user, associated with an email address.
    /// </summary>
    public class UserPublicKey : IEquatable<UserPublicKey>
    {
        private class EmailAddressComparer : IEqualityComparer<UserPublicKey>
        {
            public bool Equals(UserPublicKey? x, UserPublicKey? y)
            {
                if (x is null)
                {
                    return ReferenceEquals(y, null);
                }
                return !ReferenceEquals(y, null) && x.Email == y.Email;
            }

            public int GetHashCode(UserPublicKey upk)
            {
                if (upk == null!)
                {
                    throw new ArgumentNullException(nameof(upk));
                }

                return upk.Email.GetHashCode();
            }
        }

        public UserPublicKey(EmailAddress email, IAsymmetricPublicKey publicKey)
        {
            Email = email;
            PublicKey = publicKey;
        }

        public UserPublicKey()
        {
            Email = EmailAddress.Empty;
        }

        public static readonly IEqualityComparer<UserPublicKey> EmailComparer = new EmailAddressComparer();

        [JsonPropertyName("email")]
        public EmailAddress Email { get; set; }

        [JsonPropertyName("publickey")]
        [AllowNull]
        public IAsymmetricPublicKey PublicKey { get; set; }

        /// <summary>
        /// Indicates if this public key was imported by the user, not from the servers
        /// </summary>
        [JsonPropertyName("user_imported")]
        public bool IsUserImported { get; set; }

        public override string ToString()
        {
            return Email.ToString();
        }

        public override bool Equals(object? obj)
        {
            var other = obj as UserPublicKey;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return Email.GetHashCode() ^ PublicKey.GetHashCode();
        }

        public static bool operator ==(UserPublicKey? left, UserPublicKey? right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }
            return left.Equals(right);
        }

        public static bool operator !=(UserPublicKey? left, UserPublicKey? right)
        {
            return !(left == right);
        }

        public bool Equals(UserPublicKey? other)
        {
            if (other is null || GetType() != other.GetType())
            {
                return false;
            }

            return Email == other.Email && PublicKey != null && PublicKey.Equals(other.PublicKey);
        }
    }
}

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

using AxCrypt.Core.Crypto;
using AxCrypt.Core.Runtime;
using AxCrypt.Core.UI;

using System.Text.Json.Serialization;

namespace AxCrypt.Core.Session
{
    /// <summary>
    /// The public identifier for a LogOn identity, symmetric anonymous or asymmetric named.
    /// Used to tag resources with an identity in a non-secret way. I.e. the tag does not
    /// contain any secret information, but can be used to recognize an identity. Instances
    /// of this class are immutable.
    /// </summary>
    public class IdentityPublicTag
    {
        public static readonly IdentityPublicTag Empty = new IdentityPublicTag(SymmetricKeyThumbprint.Zero, EmailAddress.Empty);

        [JsonPropertyName("thumbprint")]
        public SymmetricKeyThumbprint ThumbprintForSerialization { get { return _thumbprint; } set { _thumbprint = value; } }

        private SymmetricKeyThumbprint _thumbprint = SymmetricKeyThumbprint.Zero;

        [JsonPropertyName("email")]
        public EmailAddress EmailAddressForSerialization { get { return _email; } set { _email = value; } }

        private EmailAddress _email = EmailAddress.Empty;

        public IdentityPublicTag()
        {
        }

        private IdentityPublicTag(SymmetricKeyThumbprint thumbprint, EmailAddress emailAddress)
        {
            _thumbprint = thumbprint;
            _email = emailAddress;
        }

        public IdentityPublicTag(LogOnIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException(nameof(identity));
            }

            _thumbprint = identity.Passphrase.Thumbprint ?? throw new InternalErrorException("Thumbprint was null.");
            _email = identity.ActiveEncryptionKeyPair.UserEmail;
        }

        /// <summary>
        /// Determines if the specified other instance matches this instance as the same identity .
        /// </summary>
        /// <param name="other">The other tag instance.</param>
        /// <returns>True if the instances are considered to represent the same identity.</returns>
        public bool Matches(IdentityPublicTag other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if (_email != EmailAddress.Empty || other._email != EmailAddress.Empty)
            {
                return _email == other._email;
            }
            return _thumbprint == other._thumbprint;
        }

        public override string ToString()
        {
            return _email.Tag + "-" + _thumbprint.ToString();
        }
    }
}

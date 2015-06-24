#region Coypright and License

/*
 * AxCrypt - Copyright 2015, Svante Seleborg, All Rights Reserved
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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Session
{
    /// <summary>
    /// The public identifier for a LogOn identity, symmetric anonymous or asymmetric named.
    /// Used to tag resources with an identity in a non-secret way. I.e. the tag does not
    /// contain any secret information, but can be used to recognize an identity. Instances
    /// of this class are immutable.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class IdentityPublicTag
    {
        public static readonly IdentityPublicTag Empty = new IdentityPublicTag();

        [JsonProperty("thumbprint")]
        private SymmetricKeyThumbprint _thumbprint;

        [JsonProperty("email")]
        private EmailAddress _email;

        private IdentityPublicTag()
        {
            _thumbprint = SymmetricKeyThumbprint.Zero;
            _email = EmailAddress.Empty;
        }

        public IdentityPublicTag(LogOnIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }

            _thumbprint = identity.Passphrase.Thumbprint;
            _email = identity.UserKeys == null ? EmailAddress.Empty : identity.UserKeys.UserEmail;
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
                throw new ArgumentNullException("other");
            }

            if (_email != EmailAddress.Empty || other._email != EmailAddress.Empty)
            {
                return _email == other._email;
            }
            return _thumbprint == other._thumbprint;
        }
    }
}
#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
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

using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Crypto.Asymmetric
{
    /// <summary>
    /// Holder of the public key for a user, associated with an e-mail.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class UserPublicKey
    {
        private UserPublicKey()
        {
        }

        public UserPublicKey(EmailAddress email, IAsymmetricPublicKey publicKey)
        {
            Email = email;
            PublicKey = publicKey;
        }

        [JsonProperty("email"), JsonConverter(typeof(EmailAddressJsonConverter))]
        public EmailAddress Email { get; private set; }

        [JsonProperty("publickey")]
        public IAsymmetricPublicKey PublicKey { get; private set; }
    }
}

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

using Axantum.AxCrypt.Core.Crypto;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;

namespace Axantum.AxCrypt.Core.Session
{
    /// <summary>
    /// A passphrase identity, associating a persisted thumbprint with an optional transient key.
    /// Instances of this class are immutable.
    /// </summary>
    [DataContract(Namespace = "http://www.axantum.com/Serialization/")]
    public class PassphraseIdentity
    {
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "This type is immutable.")]
        public static readonly PassphraseIdentity Empty = new PassphraseIdentity(Passphrase.Empty);

        public PassphraseIdentity()
        {
        }

        public PassphraseIdentity(Passphrase key)
        {
            Key = key;
            Thumbprint = Key.Thumbprint;
        }

        public Passphrase Key { get; private set; }

        [DataMember(Name = "Thumbprint")]
        public SymmetricKeyThumbprint Thumbprint { get; private set; }
    }
}
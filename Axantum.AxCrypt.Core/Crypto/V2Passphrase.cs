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

using Axantum.AxCrypt.Core.Extensions;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Crypto
{
    /// <summary>
    /// Derive a SymmetricKey from a string passphrase representation for AxCrypt V2. Instances of this class are immutable.
    /// </summary>
    public class V2Passphrase : PassphraseBase
    {
        private const long DERIVATION_ITERATIONS = 1000;

        /// <summary>
        /// Initializes a new instance of the <see cref="V2Passphrase"/> class.
        /// </summary>
        /// <param name="passphrase">The passphrase.</param>
        public V2Passphrase(string passphrase, Salt salt, long iterations, int keySize)
        {
            DerivationSalt = salt;
            DerivationIterations = iterations;
            DerivedKey = new SymmetricKey(new Pbkdf2HmacSha512(passphrase, salt, iterations).GetBytes().Reduce(keySize / 8));
            Passphrase = passphrase;
            CryptoId = CryptoId.Aes_256;
        }

        public V2Passphrase(string passphrase, int keySize)
            : this(passphrase, new Salt(256), DERIVATION_ITERATIONS, keySize)
        {
        }

        public override bool IsLegacy
        {
            get
            {
                return false;
            }
        }
    }
}
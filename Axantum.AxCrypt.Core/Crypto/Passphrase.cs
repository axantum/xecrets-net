#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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

using System;
using System.Security.Cryptography;
using System.Text;

namespace Axantum.AxCrypt.Core.Crypto
{
    /// <summary>
    /// Derive a AesKey from a string passphrase representation for AxCrypt. Instances of this class are immutable.
    /// </summary>
    public class Passphrase
    {
        private readonly AesKey _derivedPassphrase;

        /// <summary>
        /// Initializes a new instance of the <see cref="Passphrase"/> class.
        /// </summary>
        /// <param name="passphrase">The passphrase.</param>
        public Passphrase(string passphrase)
        {
            if (passphrase == null)
            {
                throw new ArgumentNullException("passphrase");
            }

            HashAlgorithm hashAlgorithm = new SHA1Managed();
            byte[] ansiBytes = Encoding.GetEncoding(1252).GetBytes(passphrase);
            byte[] hash = hashAlgorithm.ComputeHash(ansiBytes);
            byte[] derivedPassphrase = new byte[16];
            Array.Copy(hash, derivedPassphrase, derivedPassphrase.Length);

            _derivedPassphrase = new AesKey(derivedPassphrase);
        }

        /// <summary>
        /// Gets the derived passphrase AesKey instance.
        /// </summary>
        public AesKey DerivedPassphrase
        {
            get
            {
                return _derivedPassphrase;
            }
        }
    }
}
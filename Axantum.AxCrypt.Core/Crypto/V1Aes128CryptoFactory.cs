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

using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Crypto
{
    public class V1Aes128CryptoFactory : ICryptoFactory
    {
        public static readonly Guid CryptoId = new Guid("1673BBEF-A56A-43AC-AB16-E14D2BAD1CBF");

        public IDerivedKey CreateDerivedKey(Passphrase passphrase)
        {
            return new V1DerivedKey(passphrase);
        }

        public IDerivedKey RestoreDerivedKey(Passphrase passphrase, Salt salt, int derivationIterations)
        {
            return new V1DerivedKey(passphrase);
        }

        public ICrypto CreateCrypto(SymmetricKey key, SymmetricIV iv, long keyStreamOffset)
        {
            return new V1AesCrypto(this, key, iv);
        }

        public int Priority
        {
            get { return 100000; }
        }

        public Guid Id
        {
            get { return CryptoId; }
        }

        /// <summary>
        /// Gets the unique name of the algorithm implementation.
        /// </summary>
        /// <value>
        /// The name. This must be a short, language independent name usable both as an internal identifier, and as a display name.
        /// Typical values are "AES-128", "AES-256". The UI may use these as indexes for localized or clearer names, but if unknown
        /// the UI must be able to fallback and actually display this identifier as a selector for example in the UI. This is to
        /// support plug-in algorithm implementations in the future.
        /// </value>
        public string Name
        {
            get { return "AES-128-V1"; }
        }

        public int KeySize
        {
            get { return 128; }
        }

        public int BlockSize
        {
            get { return 128; }
        }
    }
}
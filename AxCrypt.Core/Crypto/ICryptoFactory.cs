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

using System;
using System.Linq;

namespace AxCrypt.Core.Crypto
{
    public delegate ICryptoFactory CryptoFactoryCreator();

    public interface ICryptoFactory
    {
        int Priority { get; }

        Guid CryptoId { get; }

        /// <summary>
        /// Gets the unique name of the algorithm implementation.
        /// </summary>
        /// <value>
        /// The name. This must be a short, language independent name usable both as an internal identifier, and as a display name.
        /// Typical values are "AES-128", "AES-256". The UI may use these as indexes for localized or clearer names, but if unknown
        /// the UI must be able to fallback and actually display this identifier as a selector for example in the UI. This is to
        /// support plug-in algorithm implementations in the future.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Gets the key size in bits
        /// </summary>
        int KeySize { get; }

        /// <summary>
        /// Gets the block size in bits
        /// </summary>
        int BlockSize { get; }

        /// <summary>
        /// Creates a new derived key, generating a random salt and an appropriate number of iterations.
        /// </summary>
        /// <param name="passphrase">The passphrase.</param>
        /// <returns>A new IDerivedKey instance.</returns>
        IDerivedKey CreateDerivedKey(Passphrase passphrase);

        /// <summary>
        /// Restores a derived key with the given salt and iterations.
        /// </summary>
        /// <param name="passphrase">The passphrase.</param>
        /// <param name="salt">The salt.</param>
        /// <param name="derivationIterations">The derivation iterations.</param>
        /// <returns>A restored IDerivedKey instance.</returns>
        IDerivedKey RestoreDerivedKey(Passphrase passphrase, Salt salt, int derivationIterations);

        /// <summary>
        /// Instantiate an approriate ICrypto implementation.
        /// </summary>
        /// <param name="key">The key to use.</param>
        /// <param name="iv">The Initial Vector to use, if relevant or null otherwise.</param>
        /// <param name="keyStreamOffset">The offset in the keystream to start it, if relevant.</param>
        /// <returns>An instance of an appropriate ICrypto implementation.</returns>
        ICrypto CreateCrypto(SymmetricKey key, SymmetricIV? iv, long keyStreamOffset);
    }
}

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
    public delegate ICryptoFactory CryptoFactoryCreator();

    public interface ICryptoFactory
    {
        int Priority { get; }

        Guid Id { get; }

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

        IDerivedKey CreatePassphrase(string passphrase);

        IDerivedKey CreatePassphrase(string passphrase, Salt salt, int derivationIterations);

        /// <summary>
        /// Instantiate an approriate ICrypto implementation.
        /// </summary>
        /// <param name="key">The key to use. It will be converted to the correct CryptoId if required.</param>
        /// <returns>An instance of an appropriate ICrypto implementation.</returns>
        ICrypto CreateCrypto(IDerivedKey key);

        /// <summary>
        /// Instantiate an approriate ICrypto implementation.
        /// </summary>
        /// <param name="key">The key to use. It will be converted to the correct CryptoId if required.</param>
        /// <param name="iv"></param>
        /// <param name="keyStreamOffset"></param>
        /// <returns>An instance of an appropriate ICrypto implementation.</returns>
        ICrypto CreateCrypto(IDerivedKey key, SymmetricIV iv, long keyStreamOffset);
    }
}
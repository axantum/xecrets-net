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
using System.Security.Cryptography;

namespace Axantum.AxCrypt.Core.Crypto
{
    public interface ICrypto : IDisposable
    {
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
        /// Gets the key associated with this instance.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        AesKey Key { get; }

        /// <summary>
        /// Create an instance of the underlying symmetric algorithm.
        /// </summary>
        /// <value>
        /// An instance of the algorithm.
        /// </value>
        SymmetricAlgorithm CreateAlgorithm();

        /// <summary>
        /// Decrypt in one operation.
        /// </summary>
        /// <param name="ciphertext">The complete cipher text</param>
        /// <returns>The decrypted result minus any padding</returns>
        byte[] Decrypt(byte[] cipherText);

        /// <summary>
        /// Encrypt in one operation
        /// </summary>
        /// <param name="plaintext">The complete plaintext bytes</param>
        /// <returns>The cipher text, complete with any padding</returns>
        byte[] Encrypt(byte[] plaintext);

        /// <summary>
        /// Using this instances parameters, create a decryptor
        /// </summary>
        /// <returns>A new decrypting transformation instance</returns>
        ICryptoTransform CreateDecryptingTransform();

        /// <summary>
        /// Using this instances parameters, create an encryptor
        /// </summary>
        /// <returns>A new encrypting transformation instance</returns>
        ICryptoTransform CreateEncryptingTransform();
    }
}
﻿#region Coypright and License

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

using AxCrypt.Abstractions.Algorithm;
using AxCrypt.Core.Algorithm;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AxCrypt.Core.Crypto
{
    public abstract class CryptoBase : ICrypto
    {
        [AllowNull]
        private SymmetricKey _key;

        /// <summary>
        /// Gets the key associated with this instance.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public SymmetricKey Key
        {
            get
            {
                return _key;
            }

            protected set
            {
                _key = value;
            }
        }

        public abstract int BlockLength { get; }

        /// <summary>
        /// Create an instance of a transform suitable for NIST Key Wrap.
        /// </summary>
        /// <param name="salt"></param>
        /// <param name="keyWrapDirection"></param>
        /// <returns></returns>
        /// <value>
        /// An instance of the transform.
        ///   </value>
        public abstract IKeyWrapTransform CreateKeyWrapTransform(Salt salt, KeyWrapDirection keyWrapDirection);

        /// <summary>
        /// Decrypt in one operation.
        /// </summary>
        /// <param name="cipherText"></param>
        /// <returns>
        /// The decrypted result minus any padding
        /// </returns>
        public abstract byte[] Decrypt(byte[] cipherText);

        /// <summary>
        /// Encrypt in one operation
        /// </summary>
        /// <param name="plaintext">The complete plaintext bytes</param>
        /// <returns>
        /// The cipher text, complete with any padding
        /// </returns>
        public abstract byte[] Encrypt(byte[] plaintext);

        /// <summary>
        /// Using this instances parameters, create a decryptor
        /// </summary>
        /// <returns>
        /// A new decrypting transformation instance
        /// </returns>
        public abstract ICryptoTransform DecryptingTransform();

        /// <summary>
        /// Using this instances parameters, create an encryptor
        /// </summary>
        /// <returns>
        /// A new encrypting transformation instance
        /// </returns>
        public abstract ICryptoTransform EncryptingTransform();
    }
}

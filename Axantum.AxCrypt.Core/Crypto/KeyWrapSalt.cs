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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Crypto
{
    /// <summary>
    /// A salt for the AES Key Wrap. Instances of this class are immutable.
    /// </summary>
    public class KeyWrapSalt
    {
        private readonly byte[] _salt;

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "The reference type 'AesIV' is, in fact, immutable.")]
        public static readonly KeyWrapSalt Zero = new KeyWrapSalt(new byte[0]);

        public KeyWrapSalt(int length)
        {
            if (!AesKey.IsValidKeyLength(length))
            {
                throw new InternalErrorException("A key wrap salt length must at least be equal to a valid AES key length.");
            }
            _salt = AxCryptEnvironment.Current.GetRandomBytes(length);
        }

        public KeyWrapSalt(byte[] salt)
        {
            if (salt == null)
            {
                throw new ArgumentNullException("salt");
            }
            if (salt.Length != 0 && !AesKey.IsValidKeyLength(salt.Length))
            {
                throw new InternalErrorException("A key wrap salt length must at least be equal to a valid AES key length.");
            }
            _salt = (byte[])salt.Clone();
        }

        public int Length
        {
            get
            {
                return _salt.Length;
            }
        }

        public byte[] GetBytes()
        {
            return (byte[])_salt.Clone();
        }
    }
}
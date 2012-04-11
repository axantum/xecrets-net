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
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.Reader;

namespace Axantum.AxCrypt.Core.Crypto
{
    /// <summary>
    /// Generates a sub key from a master key. Instances of this class are immutable.
    /// </summary>
    public class Subkey
    {
        private AesKey _subKey;

        public Subkey(AesKey masterKey, HeaderSubkey headerSubkey)
        {
            if (masterKey == null)
            {
                throw new ArgumentNullException("masterKey");
            }

            byte[] block = new byte[16];
            byte subKeyValue;
            switch (headerSubkey)
            {
                case HeaderSubkey.Hmac:
                    subKeyValue = 0;
                    break;
                case HeaderSubkey.Validator:
                    subKeyValue = 1;
                    break;
                case HeaderSubkey.Headers:
                    subKeyValue = 2;
                    break;
                case HeaderSubkey.Data:
                    subKeyValue = 3;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("headerSubkey");
            }

            block[0] = subKeyValue;
            using (AesCrypto aesCrypto = new AesCrypto(masterKey))
            {
                _subKey = new AesKey(aesCrypto.Encrypt(block));
            }
        }

        public AesKey Key
        {
            get
            {
                return _subKey;
            }
        }
    }
}
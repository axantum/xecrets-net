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

using AxCrypt.Abstractions;
using AxCrypt.Abstractions.Algorithm;
using AxCrypt.Core.Algorithm;
using AxCrypt.Core.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.IO
{
    public class V2HmacCalculator
    {
        private readonly HashAlgorithm _hmac;

        private long _count = 0;

        public V2HmacCalculator(SymmetricKey key)
        {
            _hmac = New<HMACSHA512>().Initialize(key);
        }

        private byte[]? _hmacResult = null;

        /// <summary>
        /// Get the calculated HMAC
        /// </summary>
        /// <returns>The HMAC</returns>
        public Hmac Hmac
        {
            get
            {
                if (_hmacResult == null)
                {
                    _ = _hmac.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                    byte[] result = new byte[_hmac.HashSize / 8];
                    Array.Copy(_hmac.Hash(), 0, result, 0, result.Length);
                    _hmacResult = result;
                }
                return new V2Hmac(_hmacResult);
            }
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            if (_hmacResult != null)
            {
                return;
            }
            _ = _hmac.TransformBlock(buffer, offset, count, null, 0);
            _count += count;
        }

        public long Count
        {
            get
            {
                return _count;
            }
        }
    }
}

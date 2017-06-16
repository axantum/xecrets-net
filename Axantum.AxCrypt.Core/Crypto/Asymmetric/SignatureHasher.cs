#region Coypright and License

/*
 * AxCrypt - Copyright 2017, Svante Seleborg, All Rights Reserved
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
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Abstractions.Algorithm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Crypto.Asymmetric
{
    public class SignatureHasher
    {
        public byte[] Hash(params string[] toSign)
        {
            if (toSign == null)
            {
                throw new ArgumentNullException(nameof(toSign));
            }

            IEnumerable<string> normalized = Normalize(toSign);
            IEnumerable<byte[]> encoded = Encode(normalized);

            byte[] hash = Hash(encoded);
            return hash;
        }

        private IEnumerable<string> Normalize(IEnumerable<string> toSign)
        {
            foreach (string part in toSign)
            {
                yield return Regex.Replace(part, @"\s+", string.Empty);
            }
        }

        private IEnumerable<byte[]> Encode(IEnumerable<string> normalized)
        {
            foreach (string part in normalized)
            {
                yield return Encoding.UTF8.GetBytes(part);
            }
        }

        private static byte[] Hash(IEnumerable<byte[]> encoded)
        {
            Sha256 hash = New<Sha256>();
            foreach (byte[] part in encoded)
            {
                hash.TransformBlock(part, 0, part.Length, null, 0);
            }
            hash.TransformFinalBlock(new byte[0], 0, 0);
            return hash.Hash();
        }
    }
}
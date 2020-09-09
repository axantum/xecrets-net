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
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Core.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Core.Crypto.Asymmetric
{
    public class Verifier
    {
        private IAsymmetricPublicKey _publicKey;

        public Verifier(IAsymmetricPublicKey publicKey)
        {
            _publicKey = publicKey;
        }

        public bool Verify(byte[] signature, params string[] toVerify)
        {
            if (signature == null)
            {
                throw new ArgumentNullException(nameof(signature));
            }
            if (toVerify == null)
            {
                throw new ArgumentNullException(nameof(toVerify));
            }

            byte[] hashToVerify = new SignatureHasher().Hash(toVerify);

            byte[] hash = _publicKey.TransformRaw(signature, hashToVerify.Length);

            return hash.IsEquivalentTo(0, hashToVerify, 0, hashToVerify.Length);
        }
    }
}
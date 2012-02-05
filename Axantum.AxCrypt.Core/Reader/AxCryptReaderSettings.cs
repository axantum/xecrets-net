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
 * The source is maintained at http://AxCrypt.codeplex.com/ please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Axantum.AxCrypt.Core.Reader
{
    public class AxCryptReaderSettings
    {
        private string _passphrase;

        public AxCryptReaderSettings()
        {
        }

        public AxCryptReaderSettings(string passphrase)
        {
            _passphrase = passphrase;
        }

        public byte[] GetDerivedPassphrase()
        {
            if (_passphrase == null)
            {
                return new byte[0];
            }

            HashAlgorithm hashAlgorithm = new SHA1Managed();
            byte[] ansiBytes = Encoding.GetEncoding(1252).GetBytes(_passphrase);
            byte[] hash = hashAlgorithm.ComputeHash(ansiBytes);
            byte[] derivedPassphrase = new byte[16];
            Array.Copy(hash, derivedPassphrase, derivedPassphrase.Length);

            return derivedPassphrase;
        }
    }
}
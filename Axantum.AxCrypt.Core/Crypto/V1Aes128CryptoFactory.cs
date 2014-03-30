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
    public class V1Aes128CryptoFactory : ICryptoFactory
    {
        private static readonly Guid _id = new Guid("1673BBEF-A56A-43AC-AB16-E14D2BAD1CBF");

        public IPassphrase CreatePassphrase(string passphrase)
        {
            return new V1Passphrase(passphrase);
        }

        public IPassphrase CreatePassphrase(string passphrase, Salt salt, int iterations)
        {
            return new V1Passphrase(passphrase);
        }

        public ICrypto CreateCrypto(IPassphrase key)
        {
            return new V1AesCrypto(key, SymmetricIV.Zero128);
        }

        public ICrypto CreateCrypto(IPassphrase key, SymmetricIV iv, long keyStreamOffset)
        {
            return new V1AesCrypto(key, iv);
        }

        public int Priority
        {
            get { return 100000; }
        }

        public Guid Id
        {
            get { return _id; }
        }

        public string Name
        {
            get { return "AES-128-V1"; }
        }
    }
}
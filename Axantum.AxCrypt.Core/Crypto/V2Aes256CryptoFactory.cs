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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Crypto
{
    public class V2Aes256CryptoFactory : ICryptoFactory
    {
        private static readonly Guid CRYPTOID = new Guid("E20F33D4-89E2-4D88-A39C-21DD62FB674F");

        public IDerivedKey CreateDerivedKey(Passphrase passphrase)
        {
            return new V2DerivedKey(passphrase, 256);
        }

        public IDerivedKey RestoreDerivedKey(Passphrase passphrase, Salt salt, int derivationIterations)
        {
            return new V2DerivedKey(passphrase, salt, derivationIterations, 256);
        }

        public ICrypto CreateCrypto(SymmetricKey key, SymmetricIV iv, long keyStreamOffset)
        {
            return new V2AesCrypto(key, iv, keyStreamOffset);
        }

        public int Priority
        {
            get { return 300000; }
        }

        public Guid CryptoId
        {
            get { return CRYPTOID; }
        }

        public string Name
        {
            get { return "AES-256"; }
        }

        public int KeySize
        {
            get { return 256; }
        }

        public int BlockSize
        {
            get { return 128; }
        }
    }
}
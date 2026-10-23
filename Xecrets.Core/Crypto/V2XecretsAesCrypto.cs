#region Coypright and GPL License

/*
 * Xecrets.Net - Copyright © 2022-2026, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets.Net, parts of which in turn are derived from AxCrypt as licensed under GPL v3 or later.
 *
 * However, this code is not derived from AxCrypt and is separately copyrighted and only licensed as follows unless
 * explicitly licensed otherwise. If you use any part of this code in your software, please see https://www.gnu.org/licenses/
 * for details of what this means for you.
 *
 * Xecrets.Net is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets.Net is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets.Net.  If not, see <https://www.gnu.org/licenses/>.
 *
 * The source repository can be found at https://github.com/axantum/xecrets-net please go there for more information,
 * suggestions and contributions. You may also visit https://www.axantum.com for more information about the author.
*/

#endregion Coypright and GPL License

using System.Security.Cryptography;

using AxCrypt.Core.Crypto;
using AxCrypt.Core.Extensions;
using AxCrypt.Core.Runtime;

using Xecrets.Net.Core.Crypto;
using Xecrets.Net.Cryptography;

namespace Xecrets.Core.Crypto;

// V2 AES cryptography for Xecrets.Net. This is AxCrypt.Core.Crypto.V2AesCrypto, but it adds the IKeyWrapCrypto interfac
// to perform key wrapping and unwrapping using Xecrets.Net.Cryptography.XecretsKeyWrap.
internal sealed class V2XecretsAesCrypto(SymmetricKey key, SymmetricIV? iv, long keyStreamOffset)
    : V2AesCrypto(key, iv, keyStreamOffset), IKeyWrapCrypto
{
    public byte[] KeyWrap(Salt salt, long keyWrapIterations, KeyWrapMode mode, byte[] keyMaterial)
    {
        byte[] saltedKey = SaltedKey(salt);
        try
        {
            return new XecretsKeyWrap(keyWrapIterations, mode == KeyWrapMode.AxCrypt).Wrap(saltedKey, keyMaterial);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(saltedKey);
        }
    }

    public byte[] KeyUnwrap(Salt salt, long keyWrapIterations, KeyWrapMode mode, byte[] wrapped)
    {
        byte[] saltedKey = SaltedKey(salt);
        try
        {
            return new XecretsKeyWrap(keyWrapIterations, mode == KeyWrapMode.AxCrypt).Unwrap(saltedKey, wrapped);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(saltedKey);
        }
    }

    // Salt the key encrypting key exactly as AxCrypt.Core.Crypto.BlockAlgorithmKeyWrapTransform does.
    private byte[] SaltedKey(Salt salt)
    {
        byte[] saltedKey = Key.GetBytes();
        if (salt.Length != 0 && salt.Length < saltedKey.Length)
        {
            throw new InternalErrorException("Salt is too short. It must be at least as long as the algorithm key, or empty for no salt.");
        }

        return saltedKey.Xor(salt.GetBytes().Reduce(saltedKey.Length));
    }
}

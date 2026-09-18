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

using AxCrypt.Core.Crypto;
using AxCrypt.Core.Extensions;

using Xecrets.Net.Cryptography;

namespace Xecrets.Core.Crypto;

// Derive a SymmetricKey from a string passphrase for Xecrets.Net V2. This mirrors the obsolete
// AxCrypt.Core.Crypto.V2DerivedKey, but derives with XecretsPbkdf2 and defaults to a work factor that follows
// the current recommendation. Instances of this class are immutable.
internal sealed class V2XecretsDerivedKey : DerivedKeyBase
{
    // https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html#pbkdf2
    private const int DefaultDerivationIterations = 220_000;

    public V2XecretsDerivedKey(Passphrase passphrase, Salt salt, int derivationIterations, int keySize)
    {
        DerivationSalt = salt;
        DerivationIterations = derivationIterations;
        DerivedKey = new SymmetricKey(new XecretsPbkdf2(passphrase.Text, salt.GetBytes(), derivationIterations)
            .GetBytes().Reduce(keySize / 8));
    }

    public V2XecretsDerivedKey(Passphrase passphrase, int keySize)
        : this(passphrase, new Salt(256), DefaultDerivationIterations, keySize)
    {
    }
}

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

namespace Xecrets.Core.Crypto;

// The Xecrets.Net implementation of V2 AES-256.
internal sealed class V2XecretsAes256CryptoFactory : ICryptoFactory
{
    public IDerivedKey CreateDerivedKey(Passphrase passphrase) =>
        new V2XecretsDerivedKey(passphrase, 256);

    public IDerivedKey RestoreDerivedKey(Passphrase passphrase, Salt salt, int derivationIterations) =>
        new V2XecretsDerivedKey(passphrase, salt, derivationIterations, 256);

    public ICrypto CreateCrypto(SymmetricKey key, SymmetricIV? iv, long keyStreamOffset) =>
        new V2AesCrypto(key, iv, keyStreamOffset);

    public int Priority => 300_000;

    public Guid CryptoId { get; } = new("E20F33D4-89E2-4D88-A39C-21DD62FB674F");

    public string Name => "AES-256";

    public int KeySize => 256;

    public int BlockSize => 128;
}

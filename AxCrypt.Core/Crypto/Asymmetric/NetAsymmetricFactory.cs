#region Copyright and GPL License

/*
 * Xecrets Cli - Copyright © 2022-2024, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets Cli, parts of which in turn are derived from AxCrypt as licensed under GPL v3 or later.
 * 
 * However, this code is not derived from AxCrypt and is separately copyrighted and only licensed as follows unless
 * explicitly licensed otherwise. If you use any part of this code in your software, please see https://www.gnu.org/licenses/
 * for details of what this means for you.
 *
 * Xecrets Cli is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets Cli is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets Cli.  If not, see <https://www.gnu.org/licenses/>.
 *
 * The source repository can be found at https://github.com/ please go there for more information, suggestions and
 * contributions. You may also visit https://www.axantum.com for more information about the author.
*/

#endregion Copyright and GPL Licenseusing System.Text;

using System.Text.Json.Serialization;

using AxCrypt.Core.Crypto.Asymmetric;

namespace Xecrets.Net.Core.Crypto.Asymmetric;

public class NetAsymmetricFactory : IAsymmetricFactory
{
    public IEnumerable<JsonConverter> GetConverters()
    {
        var converters = new JsonConverter[]
        {
                new NetKeyPairJsonConverter(),
                new NetPublicKeyJsonConverter(),
                new NetPrivateKeyJsonConverter(),
         };
        return converters;
    }

    public IAsymmetricPrivateKey CreatePrivateKey(string privateKeyPem)
    {
        return new NetPrivateKey(privateKeyPem);
    }

    public IAsymmetricPublicKey CreatePublicKey(string publicKeyPem)
    {
        return new NetPublicKey(publicKeyPem);
    }

    public IAsymmetricKeyPair CreateKeyPair(int bits)
    {
        return new NetKeyPair(bits);
    }

    public IAsymmetricKeyPair CreateKeyPair(string publicKeyPem, string privateKeyPem)
    {
        if (privateKeyPem == null)
        {
            throw new ArgumentNullException(nameof(privateKeyPem));
        }

        return new NetKeyPair(new NetPublicKey(publicKeyPem), privateKeyPem.Length == 0 ? null : new NetPrivateKey(privateKeyPem));
    }

    public IAsymmetricKeyPair CreateKeyPair(byte[] n, byte[] e, byte[] d, byte[] p, byte[] q, byte[] dp, byte[] dq, byte[] qinv)
    {
        return new NetKeyPair(n, e, d, p, q, dp, dq, qinv);
    }
}

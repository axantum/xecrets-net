#region Xecrets Cli Copyright and GPL License notice

/*
 * Xecrets Cli - Changes and additions Copyright © 2022-2025, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets Cli, but is derived from AxCrypt as licensed under GPL v3 or later.
 *
 * The changes and additions are separately copyrighted and only licensed under GPL v3 or later as detailed below,
 * unless explicitly licensed otherwise. If you use any part of these changes and additions in your software,
 * please see https://www.gnu.org/licenses/ for details of what this means for you.
 *
 * Warning: If you are using the original AxCrypt code under a non-GPL v3 or later license, these changes and additions
 * are not included in that license. If you use these changes under those circumstances, all your code becomes subject to
 * the GPL v3 or later license, according to the principle of strong copyleft as applied to GPL v3 or later.
 *
 * Xecrets Cli is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets Cli is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets Cli. If not, see
 * https://www.gnu.org/licenses/.
 *
 * The source repository can be found at https://github.com/axantum/xecrets-net please go there for more information,
 * suggestions and contributions, as well for commit history detailing changes and additions that fall under the strong
 * copyleft provisions mentioned above. You may also visit https://www.axantum.com for more information about the author.
*/

#endregion Xecrets Cli Copyright and GPL License notice

using System.Text.Json;

using AxCrypt.Abstractions;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.Header;
using AxCrypt.Core.Test;
using AxCrypt.Fake;

using NUnit.Framework;

using static AxCrypt.Abstractions.TypeResolve;

namespace Xecrets.Net.Core.Test;

[TestFixture(CryptoImplementation.Mono)]
[TestFixture(CryptoImplementation.WindowsDesktop)]
[TestFixture(CryptoImplementation.BouncyCastle)]
public class TestV2AsymmetricMasterKeysEncryptedHeaderBlock(CryptoImplementation cryptoImplementation)
{
    private readonly CryptoImplementation _cryptoImplementation = cryptoImplementation;

    [SetUp]
    public void Setup()
    {
        SetupAssembly.AssemblySetup(_cryptoImplementation);
    }

    [TearDown]
    public void Teardown()
    {
        TypeMap.Register.Clear();
    }

    /// <summary>
    /// The shape AxCrypt-Net (Newtonsoft) writes into a type 105 header block.
    /// </summary>
    private const string AxCryptNetMasterKeysJson = """
    {
      "masterPublicKeys": [
        {
          "pem": "-----BEGIN PUBLIC KEY-----\r\nMIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCwDVOxC9IW2MoLjd52AmKpd59d\r\nUud6NGVuecCYBdivCGzaL7WBGj1/ANWGkdr721n6eLgIk4IYFQ5suTCmp7m1AXy1\r\nt266hn+/jjbCu0Z9X8CqIgfBQRHABAslZSNk2x1D3i4KiP/29z5oUYaFivwnzjNL\r\nSoXM7ibtyihFsrqKOQIDAQAB\r\n-----END PUBLIC KEY-----\r\n"
        }
      ]
    }
    """;

    private const string AxCryptNetMasterKeyPem = "-----BEGIN PUBLIC KEY-----\r\nMIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCwDVOxC9IW2MoLjd52AmKpd59d\r\nUud6NGVuecCYBdivCGzaL7WBGj1/ANWGkdr721n6eLgIk4IYFQ5suTCmp7m1AXy1\r\nt266hn+/jjbCu0Z9X8CqIgfBQRHABAslZSNk2x1D3i4KiP/29z5oUYaFivwnzjNL\r\nSoXM7ibtyihFsrqKOQIDAQAB\r\n-----END PUBLIC KEY-----\r\n";

    [Test]
    public void TestGetSetMasterKeysAndClone()
    {
        V2AsymmetricMasterKeysEncryptedHeaderBlock headerBlock = new V2AsymmetricMasterKeysEncryptedHeaderBlock(new V2AesCrypto(SymmetricKey.Zero256, SymmetricIV.Zero128, 0));
        IAsymmetricKeyPair masterKeyPair1 = New<IAsymmetricFactory>().CreateKeyPair(1024);
        IAsymmetricKeyPair masterKeyPair2 = New<IAsymmetricFactory>().CreateKeyPair(1024);

        headerBlock.MasterKeys = new MasterKeys([masterKeyPair1.PublicKey, masterKeyPair2.PublicKey]);

        List<IAsymmetricPublicKey> masterPublicKeys = headerBlock.MasterKeys.MasterPublicKeys;
        Assert.That(masterPublicKeys, Has.Count.EqualTo(2));
        Assert.That(masterPublicKeys[0].ToString(), Is.EqualTo(masterKeyPair1.PublicKey.ToString()));
        Assert.That(masterPublicKeys[1].ToString(), Is.EqualTo(masterKeyPair2.PublicKey.ToString()));

        V2AsymmetricMasterKeysEncryptedHeaderBlock clone = (V2AsymmetricMasterKeysEncryptedHeaderBlock)headerBlock.Clone();
        masterPublicKeys = clone.MasterKeys.MasterPublicKeys;
        Assert.That(masterPublicKeys, Has.Count.EqualTo(2));
        Assert.That(masterPublicKeys[0].ToString(), Is.EqualTo(masterKeyPair1.PublicKey.ToString()));
        Assert.That(masterPublicKeys[1].ToString(), Is.EqualTo(masterKeyPair2.PublicKey.ToString()));
    }

    [Test]
    public void TestEmptyMasterKeys()
    {
        V2AsymmetricMasterKeysEncryptedHeaderBlock headerBlock = new V2AsymmetricMasterKeysEncryptedHeaderBlock(new V2AesCrypto(SymmetricKey.Zero256, SymmetricIV.Zero128, 0));

        Assert.That(headerBlock.MasterKeys.MasterPublicKeys, Is.Empty);
    }

    [Test]
    public void TestMasterKeysSerializeDeserialize()
    {
        IAsymmetricKeyPair masterKeyPair1 = New<IAsymmetricFactory>().CreateKeyPair(1024);
        IAsymmetricKeyPair masterKeyPair2 = New<IAsymmetricFactory>().CreateKeyPair(1024);
        IStringSerializer serializer = New<IStringSerializer>();

        string json = serializer.Serialize(new MasterKeys([masterKeyPair1.PublicKey, masterKeyPair2.PublicKey]));

        using (JsonDocument document = JsonDocument.Parse(json))
        {
            JsonElement keys = document.RootElement.GetProperty("masterPublicKeys");
            Assert.That(keys.ValueKind, Is.EqualTo(JsonValueKind.Array));
            Assert.That(keys.GetArrayLength(), Is.EqualTo(2));
            Assert.That(keys[0].GetProperty("pem").GetString(), Is.EqualTo(masterKeyPair1.PublicKey.ToString()));
            Assert.That(keys[1].GetProperty("pem").GetString(), Is.EqualTo(masterKeyPair2.PublicKey.ToString()));
        }

        MasterKeys? masterKeys = serializer.Deserialize<MasterKeys>(json);
        Assert.That(masterKeys, Is.Not.Null);
        Assert.That(masterKeys!.MasterPublicKeys, Has.Count.EqualTo(2));
        Assert.That(masterKeys.MasterPublicKeys[0].ToString(), Is.EqualTo(masterKeyPair1.PublicKey.ToString()));
        Assert.That(masterKeys.MasterPublicKeys[1].ToString(), Is.EqualTo(masterKeyPair2.PublicKey.ToString()));
    }

    [Test]
    public void TestDeserializeAxCryptNetJson()
    {
        MasterKeys? masterKeys = New<IStringSerializer>().Deserialize<MasterKeys>(AxCryptNetMasterKeysJson);

        Assert.That(masterKeys, Is.Not.Null);
        Assert.That(masterKeys!.MasterPublicKeys, Has.Count.EqualTo(1));

        IAsymmetricPublicKey expected = New<IAsymmetricFactory>().CreatePublicKey(AxCryptNetMasterKeyPem);
        Assert.That(masterKeys.MasterPublicKeys[0].Thumbprint, Is.EqualTo(expected.Thumbprint));
    }
}

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

using System.Text;

using AxCrypt.Abstractions;
using AxCrypt.Core;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Test;
using AxCrypt.Fake;

using NUnit.Framework;

using Xecrets.Core.Crypto;

namespace Xecrets.Net.Core.Test;

[TestFixture(CryptoImplementation.Xecrets)]
public class TestV2XecretsCryptoFactory(CryptoImplementation cryptoImplementation)
{
    [SetUp]
    public void Setup()
    {
        SetupAssembly.AssemblySetup(cryptoImplementation);
    }

    [TearDown]
    public void Teardown()
    {
        SetupAssembly.AssemblyTeardown();
    }

    [Test]
    public void TestXecretsFactoriesReplaceTheBuiltInOnes()
    {
        Assert.That(Resolve.CryptoFactory.Create(new V2Aes256CryptoFactory().CryptoId),
            Is.InstanceOf<V2XecretsAes256CryptoFactory>());
        Assert.That(Resolve.CryptoFactory.Create(new V2Aes128CryptoFactory().CryptoId),
            Is.InstanceOf<V2XecretsAes128CryptoFactory>());
    }

    [Test]
    public void TestPreferredAndMinimumResolveToXecrets()
    {
        Assert.That(Resolve.CryptoFactory.Preferred, Is.InstanceOf<V2XecretsAes256CryptoFactory>());
        Assert.That(Resolve.CryptoFactory.Minimum, Is.InstanceOf<V2XecretsAes128CryptoFactory>());
    }

    [Test]
    public void TestOrderedIdsAreUnchanged()
    {
        Guid[] withXecrets = [.. Resolve.CryptoFactory.OrderedIds];
        Guid[] withoutXecrets = [.. SetupAssembly.CreateCryptoFactory().OrderedIds];

        Assert.That(withXecrets, Is.EqualTo(withoutXecrets));
    }

    [Test]
    public void TestCreateDerivedKeyUsesTheRecommendedIterations()
    {
        IDerivedKey key = Resolve.CryptoFactory.Preferred.CreateDerivedKey(new Passphrase("secret"));

        Assert.That(key.DerivationIterations, Is.EqualTo(220000));
    }

    [Test]
    public void TestRestoreDerivedKeyHonoursTheGivenIterations()
    {
        Salt salt = new(256);
        IDerivedKey key = Resolve.CryptoFactory.Preferred.RestoreDerivedKey(new Passphrase("secret"), salt, 1000);

        Assert.That(key.DerivationIterations, Is.EqualTo(1000));
    }

    [TestCase(128)]
    [TestCase(256)]
    public void TestDerivedKeysAreIdenticalToTheAxCryptImplementation(int keySize)
    {
        Passphrase passphrase = new("a passphrase with åäö in it");
        Salt salt = new(256);

        V2DerivedKey axCryptKey = new(passphrase, salt, 1000, keySize);
        V2XecretsDerivedKey xecretsKey = new(passphrase, salt, 1000, keySize);

        Assert.That(xecretsKey.DerivedKey.GetBytes(), Is.EqualTo(axCryptKey.DerivedKey.GetBytes()));
        Assert.That(xecretsKey.DerivedKey.GetBytes().Length, Is.EqualTo(keySize / 8));
    }

    [Test]
    public void TestEncryptedWithXecretsDecryptsWithAxCrypt()
    {
        byte[] encrypted = EncryptWithRegisteredFactories("A secret in need of keeping.");

        UseAxCryptFactoriesOnly();

        Assert.That(DecryptWithRegisteredFactories(encrypted), Is.EqualTo("A secret in need of keeping."));
    }

    [Test]
    public void TestEncryptedWithAxCryptDecryptsWithXecrets()
    {
        UseAxCryptFactoriesOnly();
        byte[] encrypted = EncryptWithRegisteredFactories("A secret in need of keeping.");

        TypeMap.Register.Singleton<CryptoFactory>(() => SetupAssembly.CreateCryptoFactory(CryptoImplementation.Xecrets));

        Assert.That(DecryptWithRegisteredFactories(encrypted), Is.EqualTo("A secret in need of keeping."));
    }

    private static void UseAxCryptFactoriesOnly()
    {
        TypeMap.Register.Singleton<CryptoFactory>(() => SetupAssembly.CreateCryptoFactory());
    }

    private static byte[] EncryptWithRegisteredFactories(string text)
    {
        byte[] plainText = Encoding.UTF8.GetBytes(text);
        using MemoryStream inputStream = new(plainText);
        using MemoryStream outputStream = new();
        using (V2AxCryptDocument document = new(new EncryptionParameters(new V2Aes256CryptoFactory().CryptoId, new Passphrase("passphrase")), 113))
        {
            document.EncryptTo(inputStream, outputStream, AxCryptOptions.EncryptWithCompression);
        }

        return outputStream.ToArray();
    }

    private static string DecryptWithRegisteredFactories(byte[] encrypted)
    {
        using V2AxCryptDocument document = new();
        Assert.That(document.Load(new Passphrase("passphrase"), new V2Aes256CryptoFactory().CryptoId,
            new MemoryStream(encrypted)), Is.True);

        using MemoryStream decryptedStream = new();
        document.DecryptTo(decryptedStream);

        return Encoding.UTF8.GetString(decryptedStream.ToArray());
    }
}

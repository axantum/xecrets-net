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
using AxCrypt.Core.Runtime;
using AxCrypt.Core.Test;
using AxCrypt.Fake;

using NUnit.Framework;

using Xecrets.Core.Crypto;

namespace Xecrets.Net.Core.Test;

// Wrap with the legacy AxCrypt.Core.Crypto.KeyWrap implementation and unwrap with XecretsKeyWrap, and vice versa.
[TestFixture(CryptoImplementation.Mono)]
[TestFixture(CryptoImplementation.WindowsDesktop)]
[TestFixture(CryptoImplementation.BouncyCastle)]
public class TestXecretsKeyWrapCrossImplementation(CryptoImplementation cryptoImplementation)
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
    public void TestWrappedWithLegacyUnwrapsWithXecrets(
        [Values] KeyWrapMode mode,
        [Values(128, 256)] int keySize,
        [Values(16, 32, 48)] int keyMaterialLength,
        [Values(6, 12345)] long keyWrapIterations,
        [Values] bool salted)
    {
        byte[] keyMaterial = Pattern(keyMaterialLength, 0x40);
        Salt salt = CreateSalt(salted, keySize);

        byte[] legacyWrapped = new KeyWrap(salt, keyWrapIterations, mode).Wrap(CreateLegacyCrypto(keySize), keyMaterial);
        byte[] xecretsWrapped = new KeyWrap(salt, keyWrapIterations, mode).Wrap(CreateXecretsCrypto(keySize), keyMaterial);
        byte[] unwrapped = new KeyWrap(salt, keyWrapIterations, mode).Unwrap(CreateXecretsCrypto(keySize), legacyWrapped);

        Assert.That(xecretsWrapped, Is.EqualTo(legacyWrapped), "Both implementations should wrap identically.");
        Assert.That(unwrapped, Is.EqualTo(keyMaterial), "Xecrets should unwrap what legacy wrapped.");
    }

    [Test]
    public void TestWrappedWithXecretsUnwrapsWithLegacy(
        [Values] KeyWrapMode mode,
        [Values(128, 256)] int keySize,
        [Values(16, 32, 48)] int keyMaterialLength,
        [Values(6, 12345)] long keyWrapIterations,
        [Values] bool salted)
    {
        byte[] keyMaterial = Pattern(keyMaterialLength, 0x40);
        Salt salt = CreateSalt(salted, keySize);

        byte[] xecretsWrapped = new KeyWrap(salt, keyWrapIterations, mode).Wrap(CreateXecretsCrypto(keySize), keyMaterial);
        byte[] unwrapped = new KeyWrap(salt, keyWrapIterations, mode).Unwrap(CreateLegacyCrypto(keySize), xecretsWrapped);

        Assert.That(unwrapped, Is.EqualTo(keyMaterial), "Legacy should unwrap what Xecrets wrapped.");
    }

    [Test]
    public void TestUnwrapWithWrongSaltFailsInBothImplementations([Values] KeyWrapMode mode, [Values(128, 256)] int keySize)
    {
        byte[] keyMaterial = Pattern(32, 0x40);
        Salt salt = CreateSalt(true, keySize);

        byte[] legacyWrapped = new KeyWrap(salt, 100, mode).Wrap(CreateLegacyCrypto(keySize), keyMaterial);
        byte[] xecretsWrapped = new KeyWrap(salt, 100, mode).Wrap(CreateXecretsCrypto(keySize), keyMaterial);

        Assert.That(new KeyWrap(Salt.Zero, 100, mode).Unwrap(CreateXecretsCrypto(keySize), legacyWrapped), Is.Empty);
        Assert.That(new KeyWrap(Salt.Zero, 100, mode).Unwrap(CreateLegacyCrypto(keySize), xecretsWrapped), Is.Empty);
    }

    [Test]
    public void TestTooShortSaltFailsInBothImplementations()
    {
        Salt salt = new(Pattern(16, 0x10));
        byte[] keyMaterial = Pattern(32, 0x40);

        Assert.Throws<InternalErrorException>(() => new KeyWrap(salt, 6, KeyWrapMode.Specification).Wrap(CreateLegacyCrypto(256), keyMaterial));
        Assert.Throws<InternalErrorException>(() => new KeyWrap(salt, 6, KeyWrapMode.Specification).Wrap(CreateXecretsCrypto(256), keyMaterial));
    }

    private static V2AesCrypto CreateLegacyCrypto(int keySize) => new(new SymmetricKey(Pattern(keySize / 8, 0x00)), SymmetricIV.Zero128, 0);

    private static V2XecretsAesCrypto CreateXecretsCrypto(int keySize) => new(new SymmetricKey(Pattern(keySize / 8, 0x00)), SymmetricIV.Zero128, 0);

    private static Salt CreateSalt(bool salted, int keySize)
    {
        return salted ? new Salt(Pattern(keySize / 8, 0x10)) : Salt.Zero;
    }

    private static byte[] Pattern(int length, int start)
    {
        byte[] bytes = new byte[length];
        for (int i = 0; i < bytes.Length; ++i)
        {
            bytes[i] = (byte)(start + i);
        }

        return bytes;
    }
}

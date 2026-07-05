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
 * The source is maintained at http://bitbucket.org/AxCrypt.Desktop.Window-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
 */

#endregion Coypright and License

using AxCrypt.Core.Crypto;
using AxCrypt.Core.Extensions;
using AxCrypt.Core.Header;
using AxCrypt.Core.IO;
using AxCrypt.Core.Reader;
using AxCrypt.Core.Runtime;
using AxCrypt.Fake;

using NUnit.Framework;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

// ReSharper disable once CheckNamespace
namespace AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestKeyWrap(CryptoImplementation cryptoImplementation)
    {
        private static SymmetricKey _keyEncryptingKey;
        private static SymmetricKey _keyData;
        private static byte[] _wrapped;

        private static TestCaseData[] RoundtripCryptoFactories
        {
            get
            {
                return
                [
                    new TestCaseData(new V1Aes128CryptoFactory()).SetName("V1 AES-128"),
                    new TestCaseData(new V2Aes128CryptoFactory()).SetName("V2 AES-128"),
                    new TestCaseData(new V2Aes256CryptoFactory()).SetName("V2 AES-256"),
                ];
            }
        }

        private static TestCaseData[] SpecificationVectors
        {
            get
            {
                return
                [
                    new TestCaseData(
                            "000102030405060708090A0B0C0D0E0F".FromHex(),
                            "00112233445566778899AABBCCDDEEFF".FromHex(),
                            "1FA68B0A8112B447AEF34BD8FB5A7B829D3E862371D2CFE5".FromHex())
                        .SetName("RFC 3394 AES-128 KW"),
                    new TestCaseData(
                            "000102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1E1F".FromHex(),
                            "00112233445566778899AABBCCDDEEFF000102030405060708090A0B0C0D0E0F".FromHex(),
                            "28C9F404C4B810F4CBCCB35CFB87F8263F5786E2D80ED326CBC7F0E71A99F43BFB988B9B7A02DD21"
                                .FromHex())
                        .SetName("RFC 3394 AES-256 KW"),
                ];
            }
        }

        [SetUp]
        public void Setup()
        {
            SetupAssembly.AssemblySetup(cryptoImplementation);

            _keyEncryptingKey = new SymmetricKey([
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
                0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F,
            ]);
            _keyData = new SymmetricKey([
                0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF,
            ]);
            _wrapped =
            [
                0x1F, 0xA6, 0x8B, 0x0A, 0x81, 0x12, 0xB4, 0x47, 0xAE, 0xF3, 0x4B, 0xD8, 0xFB, 0x5A, 0x7B, 0x82,
                0x9D, 0x3E, 0x86, 0x23, 0x71, 0xD2, 0xCF, 0xE5,
            ];
        }

        [TearDown]
        public void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public void TestUnwrap()
        {
            KeyWrap keyWrap = new KeyWrap(6, KeyWrapMode.Specification);
            byte[] unwrapped = keyWrap.Unwrap(
                new V1AesCrypto(new V1Aes128CryptoFactory(), CreateV1KeyEncryptingKey(), SymmetricIV.Zero128),
                _wrapped);

            Assert.That(unwrapped, Is.EquivalentTo(_keyData.GetBytes()), "Unwrapped the wrong data");
        }

        [Test]
        public void TestWrap()
        {
            KeyWrap keyWrap = new KeyWrap(6, KeyWrapMode.Specification);
            byte[] wrapped = keyWrap.Wrap(
                new V1AesCrypto(new V1Aes128CryptoFactory(), CreateV1KeyEncryptingKey(), SymmetricIV.Zero128),
                _keyData);

            Assert.That(wrapped, Is.EquivalentTo(_wrapped),
                "The wrapped data is not correct according to specification.");

            keyWrap = new KeyWrap(6, KeyWrapMode.Specification);
            Assert.Throws<ArgumentNullException>(() =>
            {
                wrapped = keyWrap.Wrap(
                    new V1AesCrypto(new V1Aes128CryptoFactory(), CreateV1KeyEncryptingKey(), SymmetricIV.Zero128),
                    (SymmetricKey)null);
            });
        }

        [TestCaseSource(nameof(RoundtripCryptoFactories))]
        public void TestWrapAndUnwrapAxCryptMode(ICryptoFactory cryptoFactory)
        {
            SymmetricKey keyToWrap =
                new SymmetricKey([0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]);
            Salt salt = CreateSalt(cryptoFactory);
            const long keyWrapIterations = 12345;
            KeyWrap keyWrap = new KeyWrap(salt, keyWrapIterations, KeyWrapMode.AxCrypt);
            byte[] wrapped = keyWrap.Wrap(CreateCrypto(cryptoFactory), keyToWrap);
            keyWrap = new KeyWrap(salt, keyWrapIterations, KeyWrapMode.AxCrypt);
            byte[] unwrapped = keyWrap.Unwrap(CreateCrypto(cryptoFactory), wrapped);

            Assert.That(unwrapped, Is.EquivalentTo(keyToWrap.GetBytes()),
                "The unwrapped data should be equal to original.");
        }

        [TestCaseSource(nameof(RoundtripCryptoFactories))]
        public void TestWrapAndUnwrapSpecificationMode(ICryptoFactory cryptoFactory)
        {
            SymmetricKey keyToWrap =
                new SymmetricKey([0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]);
            Salt salt = CreateSalt(cryptoFactory);
            const long keyWrapIterations = 23456;
            KeyWrap keyWrap = new KeyWrap(salt, keyWrapIterations, KeyWrapMode.Specification);
            byte[] wrapped = keyWrap.Wrap(CreateCrypto(cryptoFactory), keyToWrap);
            keyWrap = new KeyWrap(salt, keyWrapIterations, KeyWrapMode.Specification);
            byte[] unwrapped = keyWrap.Unwrap(CreateCrypto(cryptoFactory), wrapped);

            Assert.That(unwrapped, Is.EquivalentTo(keyToWrap.GetBytes()),
                "The unwrapped data should be equal to original.");
        }

        [Test]
        public void TestUnwrapResourceFileKeyWithV2Aes256Password()
        {
            ICryptoFactory cryptoFactory = new V2Aes256CryptoFactory();
            Headers headers = new Headers();
            using AxCryptReader reader =
                headers.CreateReader(new LookAheadStream(OpenResource("TestCaseKeyWrap-txt.axx")));

            V2KeyWrapHeaderBlock keyWrap = headers.FindHeaderBlock<V2KeyWrapHeaderBlock>() ??
                                           throw new InvalidOperationException("Missing V2KeyWrapHeaderBlock.");
            IDerivedKey keyEncryptingKey = cryptoFactory.RestoreDerivedKey(new Passphrase("Xecrets Ez"),
                keyWrap.DerivationSalt, keyWrap.DerivationIterations);
            keyWrap.SetDerivedKey(cryptoFactory, keyEncryptingKey);

            Assert.That(keyWrap.MasterKey, Is.Not.Null, "The master key should unwrap with the expected password.");
            Assert.That(keyWrap.MasterIV, Is.Not.Null, "The master IV should unwrap with the expected password.");
            Assert.That(new V2DocumentHeaders(keyWrap).Load(headers), Is.True,
                "The document headers should validate with the unwrapped key.");
        }

        [TestCaseSource(nameof(SpecificationVectors))]
        public void TestSpecificationVectors(byte[] kek, byte[] keyData, byte[] wrappedData)
        {
            ICrypto crypto = CreateCrypto(kek);
            KeyWrap keyWrap = new KeyWrap(6, KeyWrapMode.Specification);

            byte[] wrapped = keyWrap.Wrap(crypto, keyData);
            Assert.That(wrapped, Is.EquivalentTo(wrappedData), "The wrapped data should match the RFC 3394 vector.");

            byte[] unwrapped = keyWrap.Unwrap(crypto, wrappedData);
            Assert.That(unwrapped, Is.EquivalentTo(keyData), "The unwrapped data should match the RFC 3394 vector.");
        }

        private static ICrypto CreateCrypto(ICryptoFactory cryptoFactory)
        {
            byte[] keyBytes = _keyEncryptingKey.GetBytes()[..(cryptoFactory.KeySize / 8)];
            return cryptoFactory.CreateCrypto(new SymmetricKey(keyBytes), SymmetricIV.Zero128, 0);
        }

        private static SymmetricKey CreateV1KeyEncryptingKey()
        {
            return new SymmetricKey(_keyEncryptingKey.GetBytes()[..16]);
        }

        private static ICrypto CreateCrypto(byte[] keyBytes)
        {
            return keyBytes.Length switch
            {
                16 => new V1AesCrypto(new V1Aes128CryptoFactory(), new SymmetricKey(keyBytes), SymmetricIV.Zero128),
                32 => new V2AesCrypto(new SymmetricKey(keyBytes), SymmetricIV.Zero128, 0),
                _ => throw new InvalidOperationException("Unexpected key length.")
            };
        }

        private static Salt CreateSalt(ICryptoFactory cryptoFactory)
        {
            byte[] saltBytes = new byte[cryptoFactory.KeySize / 8];
            for (int i = 0; i < saltBytes.Length; ++i)
            {
                saltBytes[i] = (byte)(i + 16);
            }

            return new Salt(saltBytes);
        }

        private static Stream OpenResource(string resourceName)
        {
            if (Xecrets.Net.Core.Test.Properties.Resources.ResourceManager.GetObject("TestCaseKeyWrap_txt") is not byte
                [] resourceBytes)
            {
                throw new InvalidOperationException($"Missing test resource '{resourceName}'.");
            }

            return new MemoryStream(resourceBytes);
        }

        [Test]
        public void TestKeyWrapConstructorWithBadArgument()
        {
            KeyWrap keyWrap = new KeyWrap(6, KeyWrapMode.Specification);
            Assert.Throws<InternalErrorException>(
                () =>
                {
                    keyWrap.Unwrap(
                        new V1AesCrypto(new V1Aes128CryptoFactory(), CreateV1KeyEncryptingKey(), SymmetricIV.Zero128),
                        _keyData.GetBytes());
                }, "Calling with too short wrapped data.");

            Assert.Throws<InternalErrorException>(() =>
            {
                keyWrap = new KeyWrap(5, KeyWrapMode.AxCrypt);
            }, "Calling with too few iterations.");

            Assert.Throws<InternalErrorException>(() =>
            {
                keyWrap = new KeyWrap(0, KeyWrapMode.AxCrypt);
            }, "Calling with zero (too few) iterations.");

            Assert.Throws<InternalErrorException>(() =>
            {
                keyWrap = new KeyWrap(-100, KeyWrapMode.AxCrypt);
            }, "Calling with negative number of iterations.");

            Assert.Throws<InternalErrorException>(() =>
            {
                keyWrap = new KeyWrap(6, (KeyWrapMode)9999);
            }, "Calling with bogus KeyWrapMode.");

            Assert.Throws<ArgumentNullException>(() =>
            {
                keyWrap = new KeyWrap(null, 6, KeyWrapMode.Specification);
            }, "Calling with null salt argument.");
        }

        [Test]
        public void TestUnwrapWithBadArgument()
        {
            KeyWrap keyWrap = new KeyWrap(100, KeyWrapMode.Specification);
            Assert.Throws<InternalErrorException>(() =>
                keyWrap.Unwrap(new V2AesCrypto(SymmetricKey.Zero256, SymmetricIV.Zero128, 0), new byte[25]));
        }

        [Test]
        public void TestWrapWithBadArgument()
        {
            KeyWrap keyWrap = new KeyWrap(100, KeyWrapMode.Specification);
            {
                byte[] nullKeyMaterial = null;
                Assert.Throws<ArgumentNullException>(() =>
                    keyWrap.Wrap(new V2AesCrypto(SymmetricKey.Zero256, SymmetricIV.Zero128, 0), nullKeyMaterial));
            }
        }
    }
}

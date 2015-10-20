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

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Header;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Reader;
using Axantum.AxCrypt.Core.Test.Properties;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestV2AxCryptReader
    {
        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup();
        }

        [TearDown]
        public static void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times"), Test]
        [TestCase(CryptoImplementation.Mono)]
        [TestCase(CryptoImplementation.WindowsDesktop)]
        [TestCase(CryptoImplementation.BouncyCastle)]
        public static void TestGetCryptoFromHeaders(CryptoImplementation cryptoImplementation)
        {
            SetupAssembly.AssemblySetupCrypto(cryptoImplementation);

            Headers headers = new Headers();
            V2DocumentHeaders documentHeaders = new V2DocumentHeaders(new EncryptionParameters(V2Aes256CryptoFactory.CryptoId, new Passphrase("passphrase")), 10);
            using (V2HmacStream<MemoryStream> stream = V2HmacStream.Create<MemoryStream>(new V2HmacCalculator(new SymmetricKey(new byte[0])), new MemoryStream()))
            {
                documentHeaders.WriteStartWithHmac(stream);
                stream.Flush();
                stream.Chained.Position = 0;

                using (V2AxCryptReader reader = new V2AxCryptReader(new LookAheadStream(stream.Chained)))
                {
                    while (reader.Read())
                    {
                        if (reader.CurrentItemType == AxCryptItemType.HeaderBlock)
                        {
                            headers.HeaderBlocks.Add(reader.CurrentHeaderBlock);
                        }
                    }
                    SymmetricKey dataEncryptingKey = documentHeaders.Headers.FindHeaderBlock<V2KeyWrapHeaderBlock>().MasterKey;
                    V2KeyWrapHeaderBlock keyWrap = headers.FindHeaderBlock<V2KeyWrapHeaderBlock>();

                    IDerivedKey key = new V2Aes256CryptoFactory().RestoreDerivedKey(new Passphrase("passphrase"), keyWrap.DerivationSalt, keyWrap.DerivationIterations);
                    keyWrap.SetDerivedKey(new V2Aes256CryptoFactory(), key);

                    Assert.That(dataEncryptingKey, Is.EqualTo(keyWrap.MasterKey));

                    key = new V2Aes256CryptoFactory().RestoreDerivedKey(new Passphrase("wrong"), keyWrap.DerivationSalt, keyWrap.DerivationIterations);
                    keyWrap.SetDerivedKey(new V2Aes256CryptoFactory(), key);

                    Assert.That(dataEncryptingKey, Is.Not.EqualTo(keyWrap.MasterKey));
                }
            }
        }

        [TestCase(CryptoImplementation.Mono)]
        [TestCase(CryptoImplementation.WindowsDesktop)]
        [TestCase(CryptoImplementation.BouncyCastle)]
        public static void TestGetOneAsymmetricCryptoFromHeaders(CryptoImplementation cryptoImplementation)
        {
            SetupAssembly.AssemblySetupCrypto(cryptoImplementation);

            Headers headers = new Headers();

            EncryptionParameters parameters = new EncryptionParameters(V2Aes256CryptoFactory.CryptoId, new Passphrase("secrets"));
            IAsymmetricPublicKey publicKey = New<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey1);
            parameters.Add(new UserPublicKey[] { new UserPublicKey(EmailAddress.Parse("test@test.com"), publicKey), });
            V2DocumentHeaders documentHeaders = new V2DocumentHeaders(parameters, 10);
            using (V2HmacStream<MemoryStream> hmacStream = V2HmacStream.Create<MemoryStream>(new V2HmacCalculator(new SymmetricKey(new byte[0])), new MemoryStream()))
            {
                documentHeaders.WriteStartWithHmac(hmacStream);
                hmacStream.Flush();
                hmacStream.Chained.Position = 0;

                using (V2AxCryptReader reader = new V2AxCryptReader(new LookAheadStream(hmacStream.Chained)))
                {
                    while (reader.Read())
                    {
                        if (reader.CurrentItemType == AxCryptItemType.HeaderBlock)
                        {
                            headers.HeaderBlocks.Add(reader.CurrentHeaderBlock);
                        }
                    }

                    SymmetricKey dataEncryptingKey = documentHeaders.Headers.FindHeaderBlock<V2KeyWrapHeaderBlock>().MasterKey;

                    V2AsymmetricKeyWrapHeaderBlock readerAsymmetricKey = headers.FindHeaderBlock<V2AsymmetricKeyWrapHeaderBlock>();

                    IAsymmetricPrivateKey privateKey1 = New<IAsymmetricFactory>().CreatePrivateKey(Resources.PrivateKey1);
                    readerAsymmetricKey.SetPrivateKey(new V2Aes256CryptoFactory(), privateKey1);
                    Assert.That(dataEncryptingKey, Is.EqualTo(readerAsymmetricKey.Crypto(0).Key), "The asymmetric wrapped key should be the one in the ICrypto instance.");

                    IAsymmetricPrivateKey privateKey2 = New<IAsymmetricFactory>().CreatePrivateKey(Resources.PrivateKey2);
                    readerAsymmetricKey.SetPrivateKey(new V2Aes256CryptoFactory(), privateKey2);
                    Assert.That(readerAsymmetricKey.Crypto(0), Is.Null, "The ICrypto instance should be null, since the private key was wrong.");
                }
            }
        }

        [TestCase(CryptoImplementation.Mono)]
        [TestCase(CryptoImplementation.WindowsDesktop)]
        [TestCase(CryptoImplementation.BouncyCastle)]
        public static void TestGetTwoAsymmetricCryptosFromHeaders(CryptoImplementation cryptoImplementation)
        {
            SetupAssembly.AssemblySetupCrypto(cryptoImplementation);

            Headers headers = new Headers();

            EncryptionParameters parameters = new EncryptionParameters(V2Aes256CryptoFactory.CryptoId, new Passphrase("secrets"));
            IAsymmetricPublicKey publicKey1 = New<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey1);
            IAsymmetricPublicKey publicKey2 = New<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey2);
            parameters.Add(new UserPublicKey[] { new UserPublicKey(EmailAddress.Parse("test1@test.com"), publicKey1), new UserPublicKey(EmailAddress.Parse("test2@test.com"), publicKey2), });
            V2DocumentHeaders documentHeaders = new V2DocumentHeaders(parameters, 10);
            using (V2HmacStream<MemoryStream> stream = V2HmacStream.Create<MemoryStream>(new V2HmacCalculator(new SymmetricKey(new byte[0])), new MemoryStream()))
            {
                documentHeaders.WriteStartWithHmac(stream);
                stream.Flush();
                stream.Chained.Position = 0;

                using (V2AxCryptReader reader = new V2AxCryptReader(new LookAheadStream(stream.Chained)))
                {
                    while (reader.Read())
                    {
                        if (reader.CurrentItemType == AxCryptItemType.HeaderBlock)
                        {
                            headers.HeaderBlocks.Add(reader.CurrentHeaderBlock);
                        }
                    }

                    SymmetricKey dataEncryptingKey = documentHeaders.Headers.FindHeaderBlock<V2KeyWrapHeaderBlock>().MasterKey;

                    IEnumerable<V2AsymmetricKeyWrapHeaderBlock> readerAsymmetricKeys = headers.HeaderBlocks.OfType<V2AsymmetricKeyWrapHeaderBlock>();
                    Assert.That(readerAsymmetricKeys.Count(), Is.EqualTo(2), "There should be two asymmetric keys in the headers.");

                    V2AsymmetricKeyWrapHeaderBlock asymmetricKey1 = readerAsymmetricKeys.First();
                    IAsymmetricPrivateKey privateKey1 = New<IAsymmetricFactory>().CreatePrivateKey(Resources.PrivateKey1);
                    asymmetricKey1.SetPrivateKey(new V2Aes256CryptoFactory(), privateKey1);
                    Assert.That(dataEncryptingKey, Is.EqualTo(asymmetricKey1.Crypto(0).Key), "The asymmetric wrapped key should be the one in the ICrypto instance.");

                    IAsymmetricPrivateKey privateKey2 = New<IAsymmetricFactory>().CreatePrivateKey(Resources.PrivateKey2);
                    asymmetricKey1.SetPrivateKey(new V2Aes256CryptoFactory(), privateKey2);
                    Assert.That(asymmetricKey1.Crypto(0), Is.Null, "The ICrypto instance should be null, since the private key was wrong.");

                    V2AsymmetricKeyWrapHeaderBlock asymmetricKey2 = readerAsymmetricKeys.Last();
                    asymmetricKey2.SetPrivateKey(new V2Aes256CryptoFactory(), privateKey2);
                    Assert.That(dataEncryptingKey, Is.EqualTo(asymmetricKey2.Crypto(0).Key), "The asymmetric wrapped key should be the one in the ICrypto instance.");

                    asymmetricKey2.SetPrivateKey(new V2Aes256CryptoFactory(), privateKey1);
                    Assert.That(asymmetricKey2.Crypto(0), Is.Null, "The ICrypto instance should be null, since the private key was wrong.");
                }
            }
        }
    }
}
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

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Header;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Reader;
using Axantum.AxCrypt.Core.Test.Properties;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

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
        [TestCase(CryptoImplementation.BouncyCastle)]
        public static void TestGetCryptoFromHeaders(CryptoImplementation cryptoImplementation)
        {
            SetupAssembly.AssemblySetupCrypto(cryptoImplementation);

            Headers headers = new Headers();
            V2DocumentHeaders documentHeaders = new V2DocumentHeaders(new EncryptionParameters(V2Aes256CryptoFactory.CryptoId, new Passphrase("passphrase")), 10);
            using (Stream chainedStream = new MemoryStream())
            {
                using (V2HmacStream stream = new V2HmacStream(new byte[0], chainedStream))
                {
                    documentHeaders.WriteStartWithHmac(stream);
                    stream.Flush();
                    chainedStream.Position = 0;

                    using (V2AxCryptReader reader = new V2AxCryptReader(chainedStream))
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
        }

        [TestCase(CryptoImplementation.Mono)]
        [TestCase(CryptoImplementation.BouncyCastle)]
        public static void TestGetOneAsymmetricCryptoFromHeaders(CryptoImplementation cryptoImplementation)
        {
            SetupAssembly.AssemblySetupCrypto(cryptoImplementation);

            Headers headers = new Headers();
            
            EncryptionParameters parameters = new EncryptionParameters(V2Aes256CryptoFactory.CryptoId, new Passphrase("secrets"));
            IAsymmetricPublicKey publicKey = TypeMap.Resolve.Singleton<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey1);
            parameters.Add(new IAsymmetricPublicKey[] { publicKey});
            V2DocumentHeaders documentHeaders = new V2DocumentHeaders(parameters, 10);
            using (Stream chainedStream = new MemoryStream())
            {
                using (V2HmacStream stream = new V2HmacStream(new byte[0], chainedStream))
                {
                    documentHeaders.WriteStartWithHmac(stream);
                    stream.Flush();
                    chainedStream.Position = 0;

                    using (V2AxCryptReader reader = new V2AxCryptReader(chainedStream))
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

                        IAsymmetricPrivateKey privateKey1 = TypeMap.Resolve.Singleton<IAsymmetricFactory>().CreatePrivateKey(Resources.PrivateKey1);
                        readerAsymmetricKey.SetPrivateKey(new V2Aes256CryptoFactory(), privateKey1);
                        Assert.That(dataEncryptingKey, Is.EqualTo(readerAsymmetricKey.Crypto(0).Key), "The asymmetric wrapped key should be the one in the ICrypto instance.");

                        IAsymmetricPrivateKey privateKey2 = TypeMap.Resolve.Singleton<IAsymmetricFactory>().CreatePrivateKey(Resources.PrivateKey2);
                        readerAsymmetricKey.SetPrivateKey(new V2Aes256CryptoFactory(), privateKey2);
                        Assert.That(readerAsymmetricKey.Crypto(0), Is.Null, "The ICrypto instance should be null, since the private key was wrong.");
                    }
                }
            }
        }

        [TestCase(CryptoImplementation.Mono)]
        [TestCase(CryptoImplementation.BouncyCastle)]
        public static void TestGetTwoAsymmetricCryptosFromHeaders(CryptoImplementation cryptoImplementation)
        {
            SetupAssembly.AssemblySetupCrypto(cryptoImplementation);

            Headers headers = new Headers();

            EncryptionParameters parameters = new EncryptionParameters(V2Aes256CryptoFactory.CryptoId, new Passphrase("secrets"));
            IAsymmetricPublicKey publicKey1 = TypeMap.Resolve.Singleton<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey1);
            IAsymmetricPublicKey publicKey2 = TypeMap.Resolve.Singleton<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey2);
            parameters.Add(new IAsymmetricPublicKey[] { publicKey1, publicKey2 });
            V2DocumentHeaders documentHeaders = new V2DocumentHeaders(parameters, 10);
            using (Stream chainedStream = new MemoryStream())
            {
                using (V2HmacStream stream = new V2HmacStream(new byte[0], chainedStream))
                {
                    documentHeaders.WriteStartWithHmac(stream);
                    stream.Flush();
                    chainedStream.Position = 0;

                    using (V2AxCryptReader reader = new V2AxCryptReader(chainedStream))
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
                        IAsymmetricPrivateKey privateKey1 = TypeMap.Resolve.Singleton<IAsymmetricFactory>().CreatePrivateKey(Resources.PrivateKey1);
                        asymmetricKey1.SetPrivateKey(new V2Aes256CryptoFactory(), privateKey1);
                        Assert.That(dataEncryptingKey, Is.EqualTo(asymmetricKey1.Crypto(0).Key), "The asymmetric wrapped key should be the one in the ICrypto instance.");

                        IAsymmetricPrivateKey privateKey2 = TypeMap.Resolve.Singleton<IAsymmetricFactory>().CreatePrivateKey(Resources.PrivateKey2);
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
}
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
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Service;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestAsymmetricUserKeysStore
    {
        private CryptoImplementation _cryptoImplementation;

        public TestAsymmetricUserKeysStore(CryptoImplementation cryptoImplementation)
        {
            _cryptoImplementation = cryptoImplementation;
        }

        [SetUp]
        public void Setup()
        {
            SetupAssembly.AssemblySetup();
            SetupAssembly.AssemblySetupCrypto(_cryptoImplementation);

            TypeMap.Register.Singleton<IRandomGenerator>(() => new FakePseudoRandomGenerator());
            TypeMap.Register.Singleton<IAsymmetricFactory>(() => new FakeAsymmetricFactory("MD5"));
        }

        [TearDown]
        public void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public void TestSimpleCreateAsymmetricKeysStore()
        {
            FakeDataStore.AddFolder(@"C:\Temp");
            IDataContainer workFolder = New<IDataContainer>(@"C:\Temp");
            AccountStorage store = new AccountStorage(new LocalAccountService(new LogOnIdentity(EmailAddress.Parse(@"svante@axantum.com"), new Passphrase("secret")), workFolder));
            UserKeyPair userKeyPair = new UserKeyPair(EmailAddress.Parse("svante@axantum.com"), 512);

            store.ImportAsync(userKeyPair).Wait();
            Assert.That(store.ActiveKeyPairAsync().Result.KeyPair.PrivateKey, Is.Not.Null);
            Assert.That(store.ActiveKeyPairAsync().Result.KeyPair.PublicKey, Is.Not.Null);
        }

        [Test]
        public void TestCreateAndLoadAsymmetricKeysStore()
        {
            FakeDataStore.AddFolder(@"C:\Temp");
            IDataContainer workFolder = New<IDataContainer>(@"C:\Temp\");
            AccountStorage store = new AccountStorage(new LocalAccountService(new LogOnIdentity(EmailAddress.Parse(@"svante@axantum.com"), new Passphrase("secret")), workFolder));
            UserKeyPair userKeyPair = new UserKeyPair(EmailAddress.Parse("svante@axantum.com"), 512);

            store.ImportAsync(userKeyPair).Wait();
            Assert.That(store.ActiveKeyPairAsync().Result.KeyPair.PrivateKey, Is.Not.Null);
            Assert.That(store.ActiveKeyPairAsync().Result.KeyPair.PublicKey, Is.Not.Null);

            IAsymmetricKeyPair keyPair = store.ActiveKeyPairAsync().Result.KeyPair;

            store = new AccountStorage(new LocalAccountService(new LogOnIdentity(EmailAddress.Parse(@"svante@axantum.com"), new Passphrase("secret")), workFolder));

            Assert.That(store.ActiveKeyPairAsync().Result.KeyPair.PrivateKey.ToString(), Is.EqualTo(keyPair.PrivateKey.ToString()));
            Assert.That(store.ActiveKeyPairAsync().Result.KeyPair.PublicKey.ToString(), Is.EqualTo(keyPair.PublicKey.ToString()));
        }

        [Test]
        public void TestEncryptCreateLoadDecryptWithAsymmetricKeysStore()
        {
            FakeDataStore.AddFolder(@"C:\Temp");
            IDataContainer workFolder = New<IDataContainer>(@"C:\Temp\");
            AccountStorage store = new AccountStorage(new LocalAccountService(new LogOnIdentity(EmailAddress.Parse(@"svante@axantum.com"), new Passphrase("secret")), workFolder));
            Resolve.KnownIdentities.DefaultEncryptionIdentity = new LogOnIdentity("secret");
            UserKeyPair userKeyPair = new UserKeyPair(EmailAddress.Parse("svante@axantum.com"), 512);

            store.ImportAsync(userKeyPair).Wait();

            string text = "AxCrypt encryption rules!";
            byte[] encryptedBytes = store.ActiveKeyPairAsync().Result.KeyPair.PublicKey.Transform(Encoding.UTF8.GetBytes(text));

            store = new AccountStorage(new LocalAccountService(new LogOnIdentity(EmailAddress.Parse(@"svante@axantum.com"), new Passphrase("secret")), workFolder));

            byte[] decryptedBytes = store.ActiveKeyPairAsync().Result.KeyPair.PrivateKey.Transform(encryptedBytes);
            Assert.That(decryptedBytes != null);
            string decryptedText = Encoding.UTF8.GetString(decryptedBytes);

            Assert.That(text, Is.EqualTo(decryptedText));
        }
    }
}
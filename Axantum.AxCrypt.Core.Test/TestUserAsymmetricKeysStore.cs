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
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestUserAsymmetricKeysStore
    {
        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup();
            Factory.Instance.Singleton<IRandomGenerator>(() => new FakePseudoRandomGenerator());
            Factory.Instance.Singleton<IAsymmetricFactory>(() => new FakeAsymmetricFactory());
            Instance.UserSettings.AsymmetricKeyBits = 512;
        }

        [TearDown]
        public static void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public static void TestSimpleCreateAsymmetricKeysStore()
        {
            FakeRuntimeFileInfo.AddFolder(@"C:\Temp");
            IRuntimeFileInfo workFolder = Factory.New<IRuntimeFileInfo>(@"C:\Temp");
            UserAsymmetricKeysStore store = new UserAsymmetricKeysStore(workFolder, Instance.KnownKeys);
            Instance.KnownKeys.DefaultEncryptionKey = new Passphrase("secret");

            store.Load(@"svante@axantum.com");
            Assert.That(store.Keys.KeyPair.PrivateKey, Is.Not.Null);
            Assert.That(store.Keys.KeyPair.PublicKey, Is.Not.Null);
        }

        [Test]
        public static void TestSaveAndLoadAsymmetricKeysStore()
        {
            FakeRuntimeFileInfo.AddFolder(@"C:\Temp");
            IRuntimeFileInfo workFolder = Factory.New<IRuntimeFileInfo>(@"C:\Temp\");
            UserAsymmetricKeysStore store = new UserAsymmetricKeysStore(workFolder, Instance.KnownKeys);
            Instance.KnownKeys.DefaultEncryptionKey = new Passphrase("secret");

            store.Load(@"svante@axantum.com");
            Assert.That(store.Keys.KeyPair.PrivateKey, Is.Not.Null);
            Assert.That(store.Keys.KeyPair.PublicKey, Is.Not.Null);
            store.Save();

            IAsymmetricKeyPair keyPair = store.Keys.KeyPair;

            store = new UserAsymmetricKeysStore(workFolder, Instance.KnownKeys);
            store.Load(@"svante@axantum.com");

            Assert.That(store.Keys.KeyPair.PrivateKey.ToString(), Is.EqualTo(keyPair.PrivateKey.ToString()));
            Assert.That(store.Keys.KeyPair.PublicKey.ToString(), Is.EqualTo(keyPair.PublicKey.ToString()));
        }

        [Test]
        public static void TestEncryptSaveLoadDecryptWithAsymmetricKeysStore()
        {
            FakeRuntimeFileInfo.AddFolder(@"C:\Temp");
            IRuntimeFileInfo workFolder = Factory.New<IRuntimeFileInfo>(@"C:\Temp\");
            UserAsymmetricKeysStore store = new UserAsymmetricKeysStore(workFolder, Instance.KnownKeys);
            Instance.KnownKeys.DefaultEncryptionKey = new Passphrase("secret");

            store.Load(@"svante@axantum.com");

            string text = "AxCrypt encryption rules!";
            byte[] encryptedBytes = store.Keys.KeyPair.PublicKey.Transform(Encoding.UTF8.GetBytes(text));

            store.Save();

            store = new UserAsymmetricKeysStore(workFolder, Instance.KnownKeys);
            store.Load(@"svante@axantum.com");

            byte[] decryptedBytes = store.Keys.KeyPair.PrivateKey.Transform(encryptedBytes);
            Assert.That(decryptedBytes != null);
            string decryptedText = Encoding.UTF8.GetString(decryptedBytes);

            Assert.That(text, Is.EqualTo(decryptedText));
        }
    }
}
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
using System.Net.Mail;
using System.Text;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestAsymmetricUserKeysStore
    {
        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup();
            TypeMap.Register.Singleton<IRandomGenerator>(() => new FakePseudoRandomGenerator());
            TypeMap.Register.Singleton<IAsymmetricFactory>(() => new FakeAsymmetricFactory("MD5"));
            Resolve.UserSettings.AsymmetricKeyBits = 512;
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
            IRuntimeFileInfo workFolder = TypeMap.Resolve.New<IRuntimeFolderInfo>(@"C:\Temp");
            UserAsymmetricKeysStore store = new UserAsymmetricKeysStore(workFolder, Resolve.KnownKeys);

            store.Create(new EmailAddress(@"svante@axantum.com"), new Passphrase("secret"));
            Assert.That(store.Keys.KeyPair.PrivateKey, Is.Not.Null);
            Assert.That(store.Keys.KeyPair.PublicKey, Is.Not.Null);
        }

        [Test]
        public static void TestCreateAndLoadAsymmetricKeysStore()
        {
            FakeRuntimeFileInfo.AddFolder(@"C:\Temp");
            IRuntimeFileInfo workFolder = TypeMap.Resolve.New<IRuntimeFolderInfo>(@"C:\Temp\");
            UserAsymmetricKeysStore store = new UserAsymmetricKeysStore(workFolder, Resolve.KnownKeys);

            store.Create(new EmailAddress(@"svante@axantum.com"), new Passphrase("secret"));
            Assert.That(store.Keys.KeyPair.PrivateKey, Is.Not.Null);
            Assert.That(store.Keys.KeyPair.PublicKey, Is.Not.Null);

            IAsymmetricKeyPair keyPair = store.Keys.KeyPair;

            store = new UserAsymmetricKeysStore(workFolder, Resolve.KnownKeys);
            store.Load(new EmailAddress(@"svante@axantum.com"), new Passphrase("secret"));

            Assert.That(Resolve.KnownKeys.DefaultEncryptionKey, Is.EqualTo(new Passphrase("secret")));
            Assert.That(store.Keys.KeyPair.PrivateKey.ToString(), Is.EqualTo(keyPair.PrivateKey.ToString()));
            Assert.That(store.Keys.KeyPair.PublicKey.ToString(), Is.EqualTo(keyPair.PublicKey.ToString()));
        }

        [Test]
        public static void TestEncryptCreateLoadDecryptWithAsymmetricKeysStore()
        {
            FakeRuntimeFileInfo.AddFolder(@"C:\Temp");
            IRuntimeFileInfo workFolder = TypeMap.Resolve.New<IRuntimeFolderInfo>(@"C:\Temp\");
            UserAsymmetricKeysStore store = new UserAsymmetricKeysStore(workFolder, Resolve.KnownKeys);
            Resolve.KnownKeys.DefaultEncryptionKey = new Passphrase("secret");

            store.Create(new EmailAddress(@"svante@axantum.com"), new Passphrase("secret"));

            string text = "AxCrypt encryption rules!";
            byte[] encryptedBytes = store.Keys.KeyPair.PublicKey.Transform(Encoding.UTF8.GetBytes(text));

            store = new UserAsymmetricKeysStore(workFolder, Resolve.KnownKeys);
            store.Load(new EmailAddress(@"svante@axantum.com"), new Passphrase("secret"));

            byte[] decryptedBytes = store.Keys.KeyPair.PrivateKey.Transform(encryptedBytes);
            Assert.That(decryptedBytes != null);
            string decryptedText = Encoding.UTF8.GetString(decryptedBytes);

            Assert.That(text, Is.EqualTo(decryptedText));
        }
    }
}
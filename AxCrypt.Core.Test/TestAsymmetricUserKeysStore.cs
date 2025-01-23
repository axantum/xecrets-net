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

using AxCrypt.Abstractions;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.IO;
using AxCrypt.Core.Service;
using AxCrypt.Core.Session;
using AxCrypt.Core.UI;
using AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xecrets.Net.Core.Test.LegacyImplementation;

using static AxCrypt.Abstractions.TypeResolve;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace AxCrypt.Core.Test
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
            SetupAssembly.AssemblySetup(_cryptoImplementation);

            TypeMap.Register.Singleton<IRandomGenerator>(() => new FakePseudoRandomGenerator());
        }

        [TearDown]
        public void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public async Task TestSimpleCreateAsymmetricKeysStore()
        {
            FakeDataStore.AddFolder(@"C:\Temp");
            IDataContainer workFolder = New<IDataContainer>(@"C:\Temp");
            AccountStorage store = new AccountStorage(new LocalAccountService(new LogOnIdentity(EmailAddress.Parse(@"svante@axantum.com"), new Passphrase("secret")), workFolder));
            UserKeyPair userKeyPair = new UserKeyPair(EmailAddress.Parse("svante@axantum.com"), 1024);

            await store.ImportAsync(userKeyPair);
            Assert.That((await store.AllKeyPairsAsync()).First().KeyPair.PrivateKey, Is.Not.Null);
            Assert.That((await store.AllKeyPairsAsync()).First().KeyPair.PublicKey, Is.Not.Null);
        }

        [Test]
        public async Task TestCreateAndLoadAsymmetricKeysStore()
        {
            FakeDataStore.AddFolder(@"C:\Temp");
            IDataContainer workFolder = New<IDataContainer>(@"C:\Temp\");
            AccountStorage store = new AccountStorage(new LocalAccountService(new LogOnIdentity(EmailAddress.Parse(@"svante@axantum.com"), new Passphrase("secret")), workFolder));
            UserKeyPair userKeyPair = new UserKeyPair(EmailAddress.Parse("svante@axantum.com"), 1024);

            await store.ImportAsync(userKeyPair);
            var allKeyPairs = await store.AllKeyPairsAsync();
            Assert.That(allKeyPairs.First().KeyPair.PrivateKey, Is.Not.Null);
            Assert.That(allKeyPairs.First().KeyPair.PublicKey, Is.Not.Null);

            IAsymmetricKeyPair keyPair = (await store.AllKeyPairsAsync()).First().KeyPair;

            store = new AccountStorage(new LocalAccountService(new LogOnIdentity(EmailAddress.Parse(@"svante@axantum.com"), new Passphrase("secret")), workFolder));

            Assert.That((await store.AllKeyPairsAsync()).First().KeyPair.PrivateKey.ToString(), Is.EqualTo(keyPair.PrivateKey.ToString()));
            Assert.That((await store.AllKeyPairsAsync()).First().KeyPair.PublicKey.ToString(), Is.EqualTo(keyPair.PublicKey.ToString()));
        }

        [Test]
        public async Task TestEncryptCreateLoadDecryptWithAsymmetricKeysStore()
        {
            FakeDataStore.AddFolder(@"C:\Temp");
            IDataContainer workFolder = New<IDataContainer>(@"C:\Temp\");
            AccountStorage store = new AccountStorage(new LocalAccountService(new LogOnIdentity(EmailAddress.Parse(@"svante@axantum.com"), new Passphrase("secret")), workFolder));
            await Resolve.KnownIdentities.SetDefaultEncryptionIdentity(new LogOnIdentity("secret"));
            UserKeyPair userKeyPair = new UserKeyPair(EmailAddress.Parse("svante@axantum.com"), 1024);

            await store.ImportAsync(userKeyPair);

            string text = "AxCrypt encryption rules!";
            byte[] encryptedBytes = (await store.AllKeyPairsAsync()).First().KeyPair.PublicKey.Transform(Encoding.UTF8.GetBytes(text));

            store = new AccountStorage(new LocalAccountService(new LogOnIdentity(EmailAddress.Parse(@"svante@axantum.com"), new Passphrase("secret")), workFolder));

            byte[] decryptedBytes = (await store.AllKeyPairsAsync()).First().KeyPair.PrivateKey.Transform(encryptedBytes);
            Assert.That(decryptedBytes != null);
            string decryptedText = Encoding.UTF8.GetString(decryptedBytes);

            Assert.That(text, Is.EqualTo(decryptedText));
        }
    }
}

#region Xecrets Cli Copyright and GPL License notice

/*
 * Xecrets Cli - Changes and additions copyright © 2022-2024, Svante Seleborg, All Rights Reserved.
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
using AxCrypt.Core.Session;
using Xecrets.Net.Core.Test.Properties;
using AxCrypt.Core.UI;
using AxCrypt.Fake;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using static AxCrypt.Abstractions.TypeResolve;
using Xecrets.Net.Core.Test.LegacyImplementation;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestKnownPublicKeys
    {
        private CryptoImplementation _cryptoImplementation;

        public TestKnownPublicKeys(CryptoImplementation cryptoImplementation)
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
        public void TestCreateSingleKey()
        {
            FakeInMemoryDataStoreItem store = new FakeInMemoryDataStoreItem("KnownPublicKeys.txt");

            IAsymmetricPublicKey key = New<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey1);
            UserPublicKey userPublicKey = new UserPublicKey(EmailAddress.Parse("test@test.com"), key);
            using (KnownPublicKeys knownPublicKeys = KnownPublicKeys.Load(store, Resolve.Serializer))
            {
                Assert.That(knownPublicKeys.PublicKeys.Count(), Is.EqualTo(0), "There should be no entries now.");

                knownPublicKeys.AddOrReplace(userPublicKey);
                Assert.That(knownPublicKeys.PublicKeys.Count(), Is.EqualTo(1), "There should be one entry now.");
            }

            using (KnownPublicKeys knownPublicKeys = KnownPublicKeys.Load(store, Resolve.Serializer))
            {
                Assert.That(knownPublicKeys.PublicKeys.Count(), Is.EqualTo(1), "There should be one entry now.");

                Assert.That(knownPublicKeys.PublicKeys.First(), Is.EqualTo(userPublicKey), "The instances should compare equal");
            }
        }

        [Test]
        public void TestReplaceKey()
        {
            FakeInMemoryDataStoreItem store = new FakeInMemoryDataStoreItem("KnownPublicKeys.txt");

            IAsymmetricPublicKey key1 = New<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey1);
            UserPublicKey userPublicKey1 = new UserPublicKey(EmailAddress.Parse("test@test.com"), key1);
            using (KnownPublicKeys knownPublicKeys = KnownPublicKeys.Load(store, Resolve.Serializer))
            {
                Assert.That(knownPublicKeys.PublicKeys.Count(), Is.EqualTo(0), "There should be no entries now.");

                knownPublicKeys.AddOrReplace(userPublicKey1);
                Assert.That(knownPublicKeys.PublicKeys.Count(), Is.EqualTo(1), "There should be one entry now.");
            }

            IAsymmetricPublicKey key2 = New<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey2);
            UserPublicKey userPublicKey2 = new UserPublicKey(EmailAddress.Parse("test@test.com"), key2);

            Assert.That(userPublicKey1, Is.Not.EqualTo(userPublicKey2), "Checking that the two public keys really are different.");
            using (KnownPublicKeys knownPublicKeys = KnownPublicKeys.Load(store, Resolve.Serializer))
            {
                Assert.That(knownPublicKeys.PublicKeys.Count(), Is.EqualTo(1), "There should be one entry now.");

                knownPublicKeys.AddOrReplace(userPublicKey2);
                Assert.That(knownPublicKeys.PublicKeys.Count(), Is.EqualTo(1), "There should be one entry now.");
                Assert.That(knownPublicKeys.PublicKeys.First(), Is.Not.EqualTo(userPublicKey1), "The instances should not compare equal");
                Assert.That(knownPublicKeys.PublicKeys.First(), Is.EqualTo(userPublicKey2), "The instances should compare equal");
            }

            using (KnownPublicKeys knownPublicKeys = KnownPublicKeys.Load(store, Resolve.Serializer))
            {
                Assert.That(knownPublicKeys.PublicKeys.Count(), Is.EqualTo(1), "There should be one entry now.");

                Assert.That(knownPublicKeys.PublicKeys.First(), Is.Not.EqualTo(userPublicKey1), "The instances should not compare equal");
                Assert.That(knownPublicKeys.PublicKeys.First(), Is.EqualTo(userPublicKey2), "The instances should compare equal");
            }
        }

        [Test]
        public void TestAddingIdentiticalPublicKey()
        {
            Mock<FakeInMemoryDataStoreItem> storeMock = new Mock<FakeInMemoryDataStoreItem>("KnownPublicKeys.txt") { CallBase = true, };

            IAsymmetricPublicKey key1 = New<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey1);
            UserPublicKey userPublicKey1 = new UserPublicKey(EmailAddress.Parse("test@test.com"), key1);
            IAsymmetricPublicKey key2 = New<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey2);
            UserPublicKey userPublicKey2 = new UserPublicKey(EmailAddress.Parse("test2@test.com"), key2);

            using (KnownPublicKeys knownPublicKeys = KnownPublicKeys.Load(storeMock.Object, Resolve.Serializer))
            {
                Assert.That(knownPublicKeys.PublicKeys.Count(), Is.EqualTo(0), "There should be no entries now.");

                knownPublicKeys.AddOrReplace(userPublicKey1);
                knownPublicKeys.AddOrReplace(userPublicKey2);
                Assert.That(knownPublicKeys.PublicKeys.Count(), Is.EqualTo(2), "There should be two entries now.");
            }
            storeMock.Verify(m => m.OpenWrite(), Times.Once);

            Assert.That(userPublicKey1, Is.Not.EqualTo(userPublicKey2), "Checking that the two public keys really are different.");

            storeMock.Invocations.Clear();
            using (KnownPublicKeys knownPublicKeys = KnownPublicKeys.Load(storeMock.Object, Resolve.Serializer))
            {
                Assert.That(knownPublicKeys.PublicKeys.Count(), Is.EqualTo(2), "There should be two entries now.");

                knownPublicKeys.AddOrReplace(userPublicKey2);
                knownPublicKeys.AddOrReplace(userPublicKey1);
                Assert.That(knownPublicKeys.PublicKeys.Count(), Is.EqualTo(2), "There should be two entries now.");
            }
            storeMock.Verify(m => m.OpenWrite(), Times.Never);
        }
    }
}

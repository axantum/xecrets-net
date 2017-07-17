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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Api.Implementation;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.Test.Properties;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using Axantum.AxCrypt.Fake;
using Axantum.AxCrypt.Mono;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public class TestSharingListViewModel
    {
        [SetUp]
        public void SetUp()
        {
            TypeMap.Register.Singleton<IAsymmetricFactory>(() => new BouncyCastleAsymmetricFactory());
            TypeMap.Register.Singleton<IEmailParser>(() => new EmailParser());
            TypeMap.Register.Singleton<AxCryptOnlineState>(() => new AxCryptOnlineState());

            TypeMap.Register.New<IStringSerializer>(() => new StringSerializer(New<IAsymmetricFactory>().GetSerializers()));
            FakeInMemoryDataStoreItem store = new FakeInMemoryDataStoreItem("KnownPublicKeys.txt");
            TypeMap.Register.New<KnownPublicKeys>(() => KnownPublicKeys.Load(store, Resolve.Serializer));
            TypeMap.Register.New<ILogging>(() => new Logging());
            TypeMap.Register.Singleton<FileLocker>(() => new FileLocker());

            New<AxCryptOnlineState>().IsOnline = true;
        }

        [TearDown]
        public void TearDown()
        {
            TypeMap.Register.Clear();
        }

        [Test]
        public void TestInitialEmptyState()
        {
            using (KnownPublicKeys knownPublicKeys = New<KnownPublicKeys>())
            {
            }
            SharingListViewModel model = new SharingListViewModel(new UserPublicKey[0], LogOnIdentity.Empty);
            Assert.That(model.SharedWith.Any(), Is.False, "There are no known public keys, and none are set as shared.");
            Assert.That(model.NotSharedWith.Any(), Is.False, "There are no known public kyes, so none can be unshared either.");
        }

        [Test]
        public void TestInitialOneKeyState()
        {
            IAsymmetricPublicKey key = New<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey1);
            UserPublicKey userPublicKey = new UserPublicKey(EmailAddress.Parse("test@test.com"), key);
            using (KnownPublicKeys knownPublicKeys = New<KnownPublicKeys>())
            {
                knownPublicKeys.AddOrReplace(userPublicKey);
            }

            SharingListViewModel model = new SharingListViewModel(new UserPublicKey[0], LogOnIdentity.Empty);
            Assert.That(model.SharedWith.Any(), Is.False, "There are no known public keys, and none are set as shared.");
            Assert.That(model.NotSharedWith.Count(), Is.EqualTo(1), "There is one known public key, so this should be available as unshared.");
        }

        [Test]
        public void TestInitialTwoKeyState()
        {
            IAsymmetricPublicKey key1 = New<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey1);
            UserPublicKey userPublicKey1 = new UserPublicKey(EmailAddress.Parse("test1@test.com"), key1);

            IAsymmetricPublicKey key2 = New<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey2);
            UserPublicKey userPublicKey2 = new UserPublicKey(EmailAddress.Parse("test2@test.com"), key2);
            using (KnownPublicKeys knownPublicKeys = New<KnownPublicKeys>())
            {
                knownPublicKeys.AddOrReplace(userPublicKey1);
                knownPublicKeys.AddOrReplace(userPublicKey2);
            }

            SharingListViewModel model = new SharingListViewModel(new UserPublicKey[0], LogOnIdentity.Empty);
            Assert.That(model.SharedWith.Any(), Is.False, "There are no known public keys, and none are set as shared.");
            Assert.That(model.NotSharedWith.Count(), Is.EqualTo(2), "There are two known public keys, so they should be available as unshared.");
        }

        [Test]
        public async Task TestMoveOneFromUnsharedToShared()
        {
            IAsymmetricPublicKey key1 = New<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey1);
            UserPublicKey userPublicKey1 = new UserPublicKey(EmailAddress.Parse("test1@test.com"), key1);

            IAsymmetricPublicKey key2 = New<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey2);
            UserPublicKey userPublicKey2 = new UserPublicKey(EmailAddress.Parse("test2@test.com"), key2);
            using (KnownPublicKeys knownPublicKeys = New<KnownPublicKeys>())
            {
                knownPublicKeys.AddOrReplace(userPublicKey1);
                knownPublicKeys.AddOrReplace(userPublicKey2);
            }

            SharingListViewModel model = new SharingListViewModel(new UserPublicKey[0], LogOnIdentity.Empty);
            Assert.That(model.SharedWith.Any(), Is.False, "There are no known public keys, and none are set as shared.");
            Assert.That(model.NotSharedWith.Count(), Is.EqualTo(2), "There are two known public keys, so they should be available as unshared.");

            await model.AsyncAddKeyShares.ExecuteAsync(new[] { userPublicKey2.Email, });
            Assert.That(model.SharedWith.Count(), Is.EqualTo(1), "One was set as shared, so there should be one here now.");
            Assert.That(model.NotSharedWith.Count(), Is.EqualTo(1), "One unshared was set as shared, so there should be only one here now.");
        }

        [Test]
        public async Task TestMoveTwoFromUnsharedToShared()
        {
            IAsymmetricPublicKey key1 = New<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey1);
            UserPublicKey userPublicKey1 = new UserPublicKey(EmailAddress.Parse("test1@test.com"), key1);

            IAsymmetricPublicKey key2 = New<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey2);
            UserPublicKey userPublicKey2 = new UserPublicKey(EmailAddress.Parse("test2@test.com"), key2);
            using (KnownPublicKeys knownPublicKeys = New<KnownPublicKeys>())
            {
                knownPublicKeys.AddOrReplace(userPublicKey1);
                knownPublicKeys.AddOrReplace(userPublicKey2);
            }

            SharingListViewModel model = new SharingListViewModel(new UserPublicKey[0], LogOnIdentity.Empty);
            Assert.That(model.SharedWith.Any(), Is.False, "There are no known public keys, and none are set as shared.");
            Assert.That(model.NotSharedWith.Count(), Is.EqualTo(2), "There are two known public keys, so they should be available as unshared.");

            await model.AsyncAddKeyShares.ExecuteAsync(new[] { userPublicKey2.Email, userPublicKey1.Email, });
            Assert.That(model.SharedWith.Count(), Is.EqualTo(2), "Two were set as shared, so there should be two here now.");
            Assert.That(model.NotSharedWith.Count(), Is.EqualTo(0), "Both unshared were set as shared, so there should be none here now.");
        }

        [Test]
        public async Task TestRemoveOneFromShared()
        {
            IAsymmetricPublicKey key1 = New<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey1);
            UserPublicKey userPublicKey1 = new UserPublicKey(EmailAddress.Parse("test1@test.com"), key1);

            IAsymmetricPublicKey key2 = New<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey2);
            UserPublicKey userPublicKey2 = new UserPublicKey(EmailAddress.Parse("test2@test.com"), key2);
            using (KnownPublicKeys knownPublicKeys = New<KnownPublicKeys>())
            {
                knownPublicKeys.AddOrReplace(userPublicKey1);
                knownPublicKeys.AddOrReplace(userPublicKey2);
            }

            SharingListViewModel model = new SharingListViewModel(new UserPublicKey[0], LogOnIdentity.Empty);
            await model.ReadyAsync();

            Assert.That(model.SharedWith.Any(), Is.False, "There are no known public keys, and none are set as shared.");
            Assert.That(model.NotSharedWith.Count(), Is.EqualTo(2), "There are two known public keys, so they should be available as unshared.");

            await model.AsyncAddKeyShares.ExecuteAsync(new[] { userPublicKey2.Email, userPublicKey1.Email, });
            Assert.That(model.SharedWith.Count(), Is.EqualTo(2), "Two were set as shared, so there should be two here now.");
            Assert.That(model.NotSharedWith.Count(), Is.EqualTo(0), "Both unshared were set as shared, so there should be none here now.");

            await model.AsyncRemoveKeyShares.ExecuteAsync(new[] { userPublicKey1, });
            Assert.That(model.SharedWith.Count(), Is.EqualTo(1), "One shared of two was removed, so there should be one here now.");
            Assert.That(model.NotSharedWith.Count(), Is.EqualTo(1), "One shared of two was removed, so there should be one here now.");
        }

        [Test]
        public async Task TestRemoveNonexistingFromShared()
        {
            IAsymmetricPublicKey key1 = New<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey1);
            UserPublicKey userPublicKey1 = new UserPublicKey(EmailAddress.Parse("test1@test.com"), key1);

            IAsymmetricPublicKey key2 = New<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey2);
            UserPublicKey userPublicKey2 = new UserPublicKey(EmailAddress.Parse("test2@test.com"), key2);
            using (KnownPublicKeys knownPublicKeys = New<KnownPublicKeys>())
            {
                knownPublicKeys.AddOrReplace(userPublicKey1);
                knownPublicKeys.AddOrReplace(userPublicKey2);
            }

            SharingListViewModel model = new SharingListViewModel(new UserPublicKey[0], LogOnIdentity.Empty);
            await model.ReadyAsync();

            Assert.That(model.SharedWith.Any(), Is.False, "There are no known public keys, and none are set as shared.");
            Assert.That(model.NotSharedWith.Count(), Is.EqualTo(2), "There are two known public keys, so they should be available as unshared.");

            await model.AsyncAddKeyShares.ExecuteAsync(new[] { userPublicKey1.Email, });
            Assert.That(model.SharedWith.Count(), Is.EqualTo(1), "One was set as shared, so there should be one here now.");
            Assert.That(model.NotSharedWith.Count(), Is.EqualTo(1), "One unshared was set as shared, so there should be one here now.");

            await model.AsyncRemoveKeyShares.ExecuteAsync(new[] { userPublicKey2, });
            Assert.That(model.SharedWith.Count(), Is.EqualTo(1), "A key that was not set as shared was attempted to remove, nothing should happen.");
            Assert.That(model.NotSharedWith.Count(), Is.EqualTo(1), "A key that was not set as shared was attempted to remove, nothing should happen.");
        }
    }
}
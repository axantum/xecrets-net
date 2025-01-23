﻿#region Xecrets Cli Copyright and GPL License notice

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
using AxCrypt.Abstractions.Algorithm;
using AxCrypt.Common;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.IO;
using AxCrypt.Core.Portable;
using AxCrypt.Core.Runtime;
using AxCrypt.Core.Session;
using Xecrets.Net.Core.Test.Properties;
using AxCrypt.Core.UI;
using AxCrypt.Core.UI.ViewModel;
using AxCrypt.Fake;
using AxCrypt.Mono;
using AxCrypt.Mono.Portable;

using NUnit.Framework;

using Xecrets.Net.Api.Implementation;
using Xecrets.Net.Core;

using static AxCrypt.Abstractions.TypeResolve;
using Xecrets.Net.Core.Crypto.Asymmetric;

namespace AxCrypt.Core.Test
{
    [TestFixture]
    public class TestSharingListViewModel
    {
        [SetUp]
        public void SetUp()
        {
            IDataStore knownPublicKeysStore = new FakeInMemoryDataStoreItem("knownpublickeys.txt");

            TypeMap.Register.Singleton<IAsymmetricFactory>(() => new NetAsymmetricFactory());
            TypeMap.Register.Singleton<IEmailParser>(() => new EmailParser());
            TypeMap.Register.Singleton<AxCryptOnlineState>(() => new AxCryptOnlineState());
            TypeMap.Register.Singleton<FileLocker>(() => new FileLocker());
            TypeMap.Register.Singleton<IPortableFactory>(() => new PortableFactory());
            TypeMap.Register.Singleton<INow>(() => new FakeNow());
            TypeMap.Register.Singleton<UserPublicKeyUpdateStatus>(() => new UserPublicKeyUpdateStatus());
            TypeMap.Register.Singleton<KnownIdentities>(() => new KnownIdentities(Resolve.FileSystemState, Resolve.SessionNotify));
            TypeMap.Register.Singleton<FileSystemState>(() => FileSystemState.Create(Resolve.WorkFolder.FileInfo.FileItemInfo("FileSystemState.txt")));
            TypeMap.Register.Singleton<WorkFolder>(() => new WorkFolder(Path.GetPathRoot(Environment.CurrentDirectory) + @"WorkFolder\"));
            TypeMap.Register.Singleton<SessionNotify>(() => new SessionNotify());
            TypeMap.Register.Singleton<UserSettingsVersion>(() => new UserSettingsVersion());
            TypeMap.Register.Singleton<ISettingsStore>(() => new SettingsStore(null));
            TypeMap.Register.Singleton<UserSettings>(() => (new FakeUserSettings(new IterationCalculator())).Initialize());

            TypeMap.Register.Singleton<IStringSerializer>(() => new SystemTextJsonStringSerializer(JsonSourceGenerationContext.CreateJsonSerializerContext(New<IAsymmetricFactory>().GetConverters())));
            TypeMap.Register.New<KnownPublicKeys>(() => KnownPublicKeys.Load(knownPublicKeysStore, Resolve.Serializer));
            TypeMap.Register.New<ILogging>(() => new Logging());
            TypeMap.Register.New<string, IDataStore>((path) => new FakeDataStore(path));
            TypeMap.Register.New<Sha256>(() => PortableFactory.SHA256Managed());
            TypeMap.Register.New<string, IDataContainer>((path) => new FakeDataContainer(path));

            New<AxCryptOnlineState>().IsOnline = true;
        }

        [TearDown]
        public void TearDown()
        {
            TypeMap.Register.Clear();
        }

        [Test]
        public async Task TestInitialEmptyState()
        {
            using (KnownPublicKeys knownPublicKeys = New<KnownPublicKeys>())
            {
            }
            SharingListViewModel model = await SharingListViewModel.CreateForFilesAsync(new string[0], LogOnIdentity.Empty);

            Assert.That(model.SharedWith.Any(), Is.False, "There are no known public keys, and none are set as shared.");
            Assert.That(model.NotSharedWith.Any(), Is.False, "There are no known public kyes, so none can be unshared either.");
        }

        [Test]
        public async Task TestInitialOneKeyState()
        {
            IAsymmetricPublicKey key = New<IAsymmetricFactory>().CreatePublicKey(Resources.PublicKey1);
            UserPublicKey userPublicKey = new UserPublicKey(EmailAddress.Parse("test@test.com"), key);
            using (KnownPublicKeys knownPublicKeys = New<KnownPublicKeys>())
            {
                knownPublicKeys.AddOrReplace(userPublicKey);
            }

            SharingListViewModel model = await SharingListViewModel.CreateForFilesAsync(new string[0], LogOnIdentity.Empty);
            Assert.That(model.SharedWith.Any(), Is.False, "There are no known public keys, and none are set as shared.");
            Assert.That(model.NotSharedWith.Count(), Is.EqualTo(1), "There is one known public key, so this should be available as unshared.");
        }

        [Test]
        public async Task TestInitialTwoKeyState()
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

            SharingListViewModel model = await SharingListViewModel.CreateForFilesAsync(new string[0], LogOnIdentity.Empty);

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

            SharingListViewModel model = await SharingListViewModel.CreateForFilesAsync(new string[0], LogOnIdentity.Empty);

            Assert.That(model.SharedWith.Any(), Is.False, "There are no known public keys, and none are set as shared.");
            Assert.That(model.NotSharedWith.Count(), Is.EqualTo(2), "There are two known public keys, so they should be available as unshared.");

            await model.AddKeyShares.ExecuteAsync(new[] { userPublicKey2.Email, });
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

            SharingListViewModel model = await SharingListViewModel.CreateForFilesAsync(new string[0], LogOnIdentity.Empty);

            Assert.That(model.SharedWith.Any(), Is.False, "There are no known public keys, and none are set as shared.");
            Assert.That(model.NotSharedWith.Count(), Is.EqualTo(2), "There are two known public keys, so they should be available as unshared.");

            await model.AddKeyShares.ExecuteAsync(new[] { userPublicKey2.Email, userPublicKey1.Email, });
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

            SharingListViewModel model = await SharingListViewModel.CreateForFilesAsync(new string[0], LogOnIdentity.Empty);

            Assert.That(model.SharedWith.Any(), Is.False, "There are no known public keys, and none are set as shared.");
            Assert.That(model.NotSharedWith.Count(), Is.EqualTo(2), "There are two known public keys, so they should be available as unshared.");

            await model.AddKeyShares.ExecuteAsync(new[] { userPublicKey2.Email, userPublicKey1.Email, });
            Assert.That(model.SharedWith.Count(), Is.EqualTo(2), "Two were set as shared, so there should be two here now.");
            Assert.That(model.NotSharedWith.Count(), Is.EqualTo(0), "Both unshared were set as shared, so there should be none here now.");

            await model.RemoveKeyShares.ExecuteAsync(new[] { userPublicKey1, });
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

            SharingListViewModel model = await SharingListViewModel.CreateForFilesAsync(new string[0], LogOnIdentity.Empty);

            Assert.That(model.SharedWith.Any(), Is.False, "There are no known public keys, and none are set as shared.");
            Assert.That(model.NotSharedWith.Count(), Is.EqualTo(2), "There are two known public keys, so they should be available as unshared.");

            await model.AddKeyShares.ExecuteAsync(new[] { userPublicKey1.Email, });
            Assert.That(model.SharedWith.Count(), Is.EqualTo(1), "One was set as shared, so there should be one here now.");
            Assert.That(model.NotSharedWith.Count(), Is.EqualTo(1), "One unshared was set as shared, so there should be one here now.");

            await model.RemoveKeyShares.ExecuteAsync(new[] { userPublicKey2, });
            Assert.That(model.SharedWith.Count(), Is.EqualTo(1), "A key that was not set as shared was attempted to remove, nothing should happen.");
            Assert.That(model.NotSharedWith.Count(), Is.EqualTo(1), "A key that was not set as shared was attempted to remove, nothing should happen.");
        }
    }
}

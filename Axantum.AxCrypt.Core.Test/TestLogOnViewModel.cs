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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.Test.Properties;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using Moq;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestLogOnViewModel
    {
        private static IList<PassphraseIdentity> _identities;

        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup();

            _identities = new List<PassphraseIdentity>();
            Mock<FileSystemState> fileSystemStateMock = new Mock<FileSystemState>();
            fileSystemStateMock.Setup<IList<PassphraseIdentity>>(f => f.Identities).Returns(_identities);
            Factory.Instance.Singleton<FileSystemState>(() => fileSystemStateMock.Object);

            Factory.Instance.Register<AxCryptFactory>(() => new AxCryptFactory());
        }

        [TearDown]
        public static void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public static void TestConstructor()
        {
            LogOnViewModel lovm = new LogOnViewModel("Identity", String.Empty, Guid.Empty);

            Assert.That(lovm.IdentityName, Is.EqualTo("Identity"));
            Assert.That(lovm.Passphrase, Is.EqualTo(""));
        }

        [Test]
        public static void TestShowPassphrase()
        {
            LogOnViewModel lovm = new LogOnViewModel("Identity", String.Empty, Guid.Empty);

            Assert.That(lovm.ShowPassphrase, Is.False);

            lovm.ShowPassphrase = true;

            Assert.That(lovm.ShowPassphrase, Is.True);
        }

        [Test]
        public static void TestValidatePropertyThatCannotBeValidated()
        {
            LogOnViewModel lovm = new LogOnViewModel("Me", String.Empty, Guid.Empty);
            string s = null;
            Assert.Throws<ArgumentException>(() => { s = lovm["ShowPassphrase"]; });
            Assert.That(s, Is.Null, "Not a real assertion, only to make the variable used for FxCop.");
        }

        [Test]
        public static void TestValidatePassphraseOk()
        {
            Mock<IUserSettings> userSettingsMock = new Mock<IUserSettings>();
            userSettingsMock.Setup<Salt>(f => f.ThumbprintSalt).Returns(Salt.Zero);
            userSettingsMock.Setup<long>(f => f.GetKeyWrapIterations(It.IsAny<Guid>())).Returns(10);
            Factory.Instance.Singleton<IUserSettings>(() => userSettingsMock.Object);

            Mock<IRuntimeEnvironment> environmentMock = new Mock<IRuntimeEnvironment>();
            environmentMock.Setup<bool>(f => f.IsLittleEndian).Returns(true);
            Factory.Instance.Singleton<IRuntimeEnvironment>(() => environmentMock.Object);

            LogOnViewModel lovm = new LogOnViewModel("Me", String.Empty, Guid.Empty);

            _identities.Add(new PassphraseIdentity("Me", new V1Passphrase("abc1234")));

            lovm.Passphrase = "abc1234";

            Assert.That(lovm["Passphrase"], Is.EqualTo(""));
            Assert.That(lovm.ValidationError, Is.EqualTo((int)ValidationError.None));
        }

        [Test]
        public static void TestValidatePassphraseNotOk()
        {
            Mock<IUserSettings> userSettingsMock = new Mock<IUserSettings>();
            userSettingsMock.Setup<Salt>(f => f.ThumbprintSalt).Returns(Salt.Zero);
            userSettingsMock.Setup<long>(f => f.GetKeyWrapIterations(It.IsAny<Guid>())).Returns(10);
            Factory.Instance.Singleton<IUserSettings>(() => userSettingsMock.Object);

            Mock<IRuntimeEnvironment> environmentMock = new Mock<IRuntimeEnvironment>();
            environmentMock.Setup<bool>(f => f.IsLittleEndian).Returns(true);
            Factory.Instance.Singleton<IRuntimeEnvironment>(() => environmentMock.Object);

            LogOnViewModel lovm = new LogOnViewModel("Me", String.Empty, Guid.Empty);

            _identities.Add(new PassphraseIdentity("Me", new V1Passphrase("abc1234")));

            lovm.Passphrase = "abc12345";

            Assert.That(lovm["Passphrase"], Is.Not.EqualTo(""));
            Assert.That(lovm.ValidationError, Is.EqualTo((int)ValidationError.WrongPassphrase));
        }

        [Test]
        public static void TestValidateNonExistingPropertyName()
        {
            LogOnViewModel lovm = new LogOnViewModel("Me", String.Empty, Guid.Empty);
            string s = null;
            Assert.Throws<ArgumentException>(() => { s = lovm["NonExisting"]; });
            Assert.That(s, Is.Null, "Not a real assertion, only to make the variable used for FxCop.");
        }

        [Test]
        public static void TestValidateWrongPassphraseWithRealFile()
        {
            _identities.Add(new PassphraseIdentity(Environment.UserName, new V1Passphrase("a")));

            FakeRuntimeFileInfo.AddFile(@"C:\My Folder\MyFile-txt.axx", new MemoryStream(Resources.helloworld_key_a_txt));
            LogOnViewModel npvm = new LogOnViewModel(Environment.UserName, @"C:\My Folder\MyFile-txt.axx", Guid.Empty);
            npvm.Passphrase = "b";

            Assert.That(npvm["Passphrase"], Is.Not.EqualTo(""));
            Assert.That(npvm.ValidationError, Is.EqualTo((int)ValidationError.WrongPassphrase));
        }

        [Test]
        public static void TestValidateCorrectPassphraseWithRealFile()
        {
            _identities.Add(new PassphraseIdentity(Environment.UserName, new V1Passphrase("a")));

            FakeRuntimeFileInfo.AddFile(@"C:\My Folder\MyFile-txt.axx", new MemoryStream(Resources.helloworld_key_a_txt));
            LogOnViewModel npvm = new LogOnViewModel(Environment.UserName, @"C:\My Folder\MyFile-txt.axx", Guid.Empty);
            npvm.Passphrase = "a";

            Assert.That(npvm["Passphrase"], Is.EqualTo(""));
            Assert.That(npvm.ValidationError, Is.EqualTo((int)ValidationError.None));
        }

        [Test]
        public static void TestValidateWrongButKnownPassphraseWithRealFile()
        {
            _identities.Add(new PassphraseIdentity(Environment.UserName, new V1Passphrase("b")));

            FakeRuntimeFileInfo.AddFile(@"C:\My Folder\MyFile-txt.axx", new MemoryStream(Resources.helloworld_key_a_txt));
            LogOnViewModel npvm = new LogOnViewModel(Environment.UserName, @"C:\My Folder\MyFile-txt.axx", Guid.Empty);
            npvm.Passphrase = "b";

            Assert.That(npvm["Passphrase"], Is.Not.EqualTo(""));
            Assert.That(npvm.ValidationError, Is.EqualTo((int)ValidationError.WrongPassphrase));
        }
    }
}
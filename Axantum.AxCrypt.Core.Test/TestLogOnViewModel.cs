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
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestLogOnViewModel
    {
        private static IList<PassphraseIdentity> _identities;

        [SetUp]
        public static void Setup()
        {
            _identities = new List<PassphraseIdentity>();
            Mock<FileSystemState> fileSystemStateMock = new Mock<FileSystemState>();
            fileSystemStateMock.Setup<IList<PassphraseIdentity>>(f => f.Identities).Returns(_identities);
            Factory.Instance.Singleton<FileSystemState>(() => fileSystemStateMock.Object);
        }

        [TearDown]
        public static void Teardown()
        {
            Factory.Instance.Clear();
        }

        [Test]
        public static void TestConstructor()
        {
            LogOnViewModel lovm = new LogOnViewModel("Identity");

            Assert.That(lovm.IdentityName, Is.EqualTo("Identity"));
            Assert.That(lovm.Passphrase, Is.EqualTo(""));
        }

        [Test]
        public static void TestShowPassphrase()
        {
            LogOnViewModel lovm = new LogOnViewModel("Identity");

            Assert.That(lovm.ShowPassphrase, Is.False);

            lovm.ShowPassphrase = true;

            Assert.That(lovm.ShowPassphrase, Is.True);
        }

        [Test]
        public static void TestValidatePropertyThatCannotBeValidated()
        {
            LogOnViewModel lovm = new LogOnViewModel("Me");
            string s = null;
            Assert.Throws<ArgumentException>(() => { s = lovm["ShowPassphrase"]; });
            Assert.That(s, Is.Null, "Not a real assertion, only to make the variable used for FxCop.");
        }

        [Test]
        public static void TestValidatePassphraseOk()
        {
            Mock<IUserSettings> userSettingsMock = new Mock<IUserSettings>();
            userSettingsMock.Setup<KeyWrapSalt>(f => f.ThumbprintSalt).Returns(KeyWrapSalt.Zero);
            userSettingsMock.Setup<long>(f => f.V1KeyWrapIterations).Returns(10);
            Factory.Instance.Singleton<IUserSettings>(() => userSettingsMock.Object);

            Mock<IRuntimeEnvironment> environmentMock = new Mock<IRuntimeEnvironment>();
            environmentMock.Setup<bool>(f => f.IsLittleEndian).Returns(true);
            Factory.Instance.Singleton<IRuntimeEnvironment>(() => environmentMock.Object);

            LogOnViewModel lovm = new LogOnViewModel("Me");

            _identities.Add(new PassphraseIdentity("Me", new V1Passphrase("abc1234").DerivedKey));

            lovm.Passphrase = "abc1234";

            Assert.That(lovm["Passphrase"], Is.EqualTo(""));
            Assert.That(lovm.ValidationError, Is.EqualTo((int)ValidationError.None));
        }

        [Test]
        public static void TestValidatePassphraseNotOk()
        {
            Mock<IUserSettings> userSettingsMock = new Mock<IUserSettings>();
            userSettingsMock.Setup<KeyWrapSalt>(f => f.ThumbprintSalt).Returns(KeyWrapSalt.Zero);
            userSettingsMock.Setup<long>(f => f.V1KeyWrapIterations).Returns(10);
            Factory.Instance.Singleton<IUserSettings>(() => userSettingsMock.Object);

            Mock<IRuntimeEnvironment> environmentMock = new Mock<IRuntimeEnvironment>();
            environmentMock.Setup<bool>(f => f.IsLittleEndian).Returns(true);
            Factory.Instance.Singleton<IRuntimeEnvironment>(() => environmentMock.Object);

            LogOnViewModel lovm = new LogOnViewModel("Me");

            _identities.Add(new PassphraseIdentity("Me", new V1Passphrase("abc1234").DerivedKey));

            lovm.Passphrase = "abc12345";

            Assert.That(lovm["Passphrase"], Is.Not.EqualTo(""));
            Assert.That(lovm.ValidationError, Is.EqualTo((int)ValidationError.WrongPassphrase));
        }

        [Test]
        public static void TestValidateNonExistingPropertyName()
        {
            LogOnViewModel lovm = new LogOnViewModel("Me");
            string s = null;
            Assert.Throws<ArgumentException>(() => { s = lovm["NonExisting"]; });
            Assert.That(s, Is.Null, "Not a real assertion, only to make the variable used for FxCop.");
        }
    }
}
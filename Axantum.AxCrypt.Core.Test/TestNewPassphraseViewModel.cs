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

using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI.ViewModel;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestNewPassphraseViewModel
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
        public static void TestConstructorWithoutKnownDefaultIdentity()
        {
            NewPassphraseViewModel npvm = new NewPassphraseViewModel("Identity");

            Assert.That(npvm.IdentityName, Is.EqualTo("Identity"));
            Assert.That(npvm.Passphrase, Is.EqualTo(""));
        }

        [Test]
        public static void TestConstructorWithKnownDefaultIdentity()
        {
            _identities.Add(new PassphraseIdentity(Environment.UserName));
            NewPassphraseViewModel npvm = new NewPassphraseViewModel(Environment.UserName);

            Assert.That(npvm.IdentityName, Is.EqualTo(""));
            Assert.That(npvm.Passphrase, Is.EqualTo(""));
        }

        [Test]
        public static void TestShowPassphrase()
        {
            NewPassphraseViewModel npvm = new NewPassphraseViewModel("Identity");

            Assert.That(npvm.ShowPassphrase, Is.False);

            npvm.ShowPassphrase = true;

            Assert.That(npvm.ShowPassphrase, Is.True);
        }

        [Test]
        public static void TestValidateIdentity()
        {
            NewPassphraseViewModel npvm = new NewPassphraseViewModel("Identity");

            Assert.That(npvm["IdentityName"], Is.EqualTo(""));
            Assert.That(npvm.ValidationError, Is.EqualTo((int)ValidationError.None));
        }

        [Test]
        public static void TestValidateIdentityAlreadyExistsFails()
        {
            _identities.Add(new PassphraseIdentity("Identity"));
            NewPassphraseViewModel npvm = new NewPassphraseViewModel(Environment.UserName);
            npvm.IdentityName = "Identity";

            Assert.That(npvm["IdentityName"], Is.Not.EqualTo(""));
            Assert.That(npvm.ValidationError, Is.EqualTo((int)ValidationError.IdentityExistsAlready));
        }

        [Test]
        public static void TestValidatePropertyThatCannotBeValidated()
        {
            NewPassphraseViewModel npvm = new NewPassphraseViewModel(Environment.UserName);
            Assert.Throws<ArgumentException>(() => { string s = npvm["ShowPassphrase"]; });
        }

        [Test]
        public static void TestValidatePassphraseOk()
        {
            NewPassphraseViewModel npvm = new NewPassphraseViewModel(Environment.UserName);

            npvm.Passphrase = "abc1234";
            npvm.Verification = "abc1234";

            Assert.That(npvm["Passphrase"], Is.EqualTo(""));
            Assert.That(npvm["Verification"], Is.EqualTo(""));
            Assert.That(npvm.ValidationError, Is.EqualTo((int)ValidationError.None));
        }

        [Test]
        public static void TestValidatePassphraseNotOk()
        {
            NewPassphraseViewModel npvm = new NewPassphraseViewModel(Environment.UserName);

            npvm.Passphrase = "abc1234";
            npvm.Verification = "abc12345";

            Assert.That(npvm["Passphrase"], Is.Not.EqualTo(""));
            Assert.That(npvm["Verification"], Is.Not.EqualTo(""));
            Assert.That(npvm.ValidationError, Is.EqualTo((int)ValidationError.VerificationPassphraseWrong));
        }

        [Test]
        public static void TestValidateNonExistingPropertyName()
        {
            NewPassphraseViewModel npvm = new NewPassphraseViewModel(Environment.UserName);
            Assert.Throws<ArgumentException>(() => { string s = npvm["NonExisting"]; });
        }
    }
}
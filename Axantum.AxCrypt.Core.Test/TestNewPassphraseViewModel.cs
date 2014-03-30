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
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.Test.Properties;
using Axantum.AxCrypt.Core.UI.ViewModel;
using Moq;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestNewPassphraseViewModel
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
        }

        [TearDown]
        public static void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public static void TestConstructorWithoutKnownDefaultIdentity()
        {
            NewPassphraseViewModel npvm = new NewPassphraseViewModel(String.Empty, "Identity", String.Empty);

            Assert.That(npvm.IdentityName, Is.EqualTo("Identity"));
            Assert.That(npvm.Passphrase, Is.EqualTo(""));
        }

        [Test]
        public static void TestConstructorWithKnownDefaultIdentity()
        {
            _identities.Add(new PassphraseIdentity(Environment.UserName, new V1Aes128CryptoFactory().Id));
            NewPassphraseViewModel npvm = new NewPassphraseViewModel(String.Empty, Environment.UserName, String.Empty);

            Assert.That(npvm.IdentityName, Is.EqualTo(String.Empty));
            Assert.That(npvm.Passphrase, Is.EqualTo(String.Empty));
        }

        [Test]
        public static void TestShowPassphrase()
        {
            NewPassphraseViewModel npvm = new NewPassphraseViewModel("Identity", String.Empty, String.Empty);

            Assert.That(npvm.ShowPassphrase, Is.False);

            npvm.ShowPassphrase = true;

            Assert.That(npvm.ShowPassphrase, Is.True);
        }

        [Test]
        public static void TestValidateIdentity()
        {
            NewPassphraseViewModel npvm = new NewPassphraseViewModel("Identity", String.Empty, String.Empty);

            Assert.That(npvm["IdentityName"], Is.EqualTo(""));
            Assert.That(npvm.ValidationError, Is.EqualTo((int)ValidationError.None));
        }

        [Test]
        public static void TestValidateIdentityAlreadyExistsFails()
        {
            _identities.Add(new PassphraseIdentity("Identity", new V2Aes256CryptoFactory().Id));
            NewPassphraseViewModel npvm = new NewPassphraseViewModel(Environment.UserName, String.Empty, String.Empty);
            npvm.IdentityName = "Identity";

            Assert.That(npvm["IdentityName"], Is.Not.EqualTo(""));
            Assert.That(npvm.ValidationError, Is.EqualTo((int)ValidationError.IdentityExistsAlready));
        }

        [Test]
        public static void TestValidatePropertyThatCannotBeValidated()
        {
            NewPassphraseViewModel npvm = new NewPassphraseViewModel(Environment.UserName, String.Empty, String.Empty);
            string s = null;
            Assert.Throws<ArgumentException>(() => { s = npvm["ShowPassphrase"]; });
            Assert.That(s, Is.Null, "Not a real assertion, only to make the variable used for FxCop.");
        }

        [Test]
        public static void TestValidatePassphraseOk()
        {
            Factory.Instance.Register<AxCryptFactory>(() => new AxCryptFactory());

            NewPassphraseViewModel npvm = new NewPassphraseViewModel(Environment.UserName, String.Empty, String.Empty);

            npvm.Passphrase = "abc1234";
            npvm.Verification = "abc1234";

            Assert.That(npvm["Passphrase"], Is.EqualTo(""));
            Assert.That(npvm["Verification"], Is.EqualTo(""));
            Assert.That(npvm.ValidationError, Is.EqualTo((int)ValidationError.None));
        }

        [Test]
        public static void TestValidatePassphraseNotOk()
        {
            NewPassphraseViewModel npvm = new NewPassphraseViewModel(Environment.UserName, String.Empty, String.Empty);

            npvm.Passphrase = "abc1234";
            npvm.Verification = "abc12345";

            Assert.That(npvm["Verification"], Is.Not.EqualTo(""));
            Assert.That(npvm.ValidationError, Is.EqualTo((int)ValidationError.VerificationPassphraseWrong));
        }

        [Test]
        public static void TestValidateNonExistingPropertyName()
        {
            NewPassphraseViewModel npvm = new NewPassphraseViewModel(Environment.UserName, String.Empty, String.Empty);
            string s = null;
            Assert.Throws<ArgumentException>(() => { s = npvm["NonExisting"]; });
            Assert.That(s, Is.Null, "Not a real assertion, only to make the variable used for FxCop.");
        }

        [Test]
        public static void TestValidateWrongPassphraseWithRealFile()
        {
            FakeRuntimeFileInfo.AddFile(@"C:\My Folder\MyFile-txt.axx", new MemoryStream(Resources.helloworld_key_a_txt));
            NewPassphraseViewModel npvm = new NewPassphraseViewModel(Environment.UserName, String.Empty, @"C:\My Folder\MyFile-txt.axx");
            npvm.Passphrase = "b";
            npvm.Verification = "b";

            Assert.That(npvm["Passphrase"], Is.Not.EqualTo(""));
            Assert.That(npvm.ValidationError, Is.EqualTo((int)ValidationError.WrongPassphrase));
        }
    }
}
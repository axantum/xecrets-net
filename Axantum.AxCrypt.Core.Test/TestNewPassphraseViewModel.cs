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
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.Test.Properties;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using Axantum.AxCrypt.Fake;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestNewPassphraseViewModel
    {
        private static IList<Passphrase> _identities;

        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup();

            _identities = new List<Passphrase>();
            Mock<FileSystemState> fileSystemStateMock = new Mock<FileSystemState>();
            fileSystemStateMock.Setup<IList<Passphrase>>(f => f.KnownPassphrases).Returns(_identities);
            TypeMap.Register.Singleton<FileSystemState>(() => fileSystemStateMock.Object);
        }

        [TearDown]
        public static void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public static void TestConstructorWithoutKnownDefaultIdentity()
        {
            NewPasswordViewModel npvm = new NewPasswordViewModel(String.Empty, String.Empty);

            Assert.That(npvm.Password, Is.EqualTo(""));
        }

        [TestCase(CryptoImplementation.Mono)]
        [TestCase(CryptoImplementation.WindowsDesktop)]
        [TestCase(CryptoImplementation.BouncyCastle)]
        public static void TestConstructorWithKnownDefaultIdentity(CryptoImplementation cryptoImplementation)
        {
            SetupAssembly.AssemblySetupCrypto(cryptoImplementation);

            _identities.Add(Passphrase.Empty);
            NewPasswordViewModel npvm = new NewPasswordViewModel(String.Empty, String.Empty);

            Assert.That(npvm.Password, Is.EqualTo(String.Empty));
        }

        [Test]
        public static void TestShowPassphrase()
        {
            NewPasswordViewModel npvm = new NewPasswordViewModel("Identity", String.Empty);

            Assert.That(npvm.ShowPassword, Is.False);

            npvm.ShowPassword = true;

            Assert.That(npvm.ShowPassword, Is.True);
        }

        [Test]
        public static void TestValidateIdentity()
        {
            NewPasswordViewModel npvm = new NewPasswordViewModel("Identity", String.Empty);

            Assert.That(npvm.ValidationError, Is.EqualTo((int)ValidationError.None));
        }

        [Test]
        public static void TestValidatePropertyThatCannotBeValidated()
        {
            NewPasswordViewModel npvm = new NewPasswordViewModel(String.Empty, String.Empty);
            string s = null;
            Assert.Throws<ArgumentException>(() => { s = npvm[nameof(NewPasswordViewModel.ShowPassword)]; });
            Assert.That(s, Is.Null, "Not a real assertion, only to make the variable used for FxCop.");
        }

        [Test]
        public static void TestValidatePassphraseOk()
        {
            TypeMap.Register.New<AxCryptFactory>(() => new AxCryptFactory());
            TypeMap.Register.Singleton<PasswordStrengthEvaluator>(() => new PasswordStrengthEvaluator(64, 0));

            NewPasswordViewModel npvm = new NewPasswordViewModel(String.Empty, String.Empty);

            npvm.Password = "Loadiney4aropRout[";
            npvm.Verification = "Loadiney4aropRout[";

            Assert.That(npvm[nameof(NewPasswordViewModel.Password)], Is.EqualTo(""));
            Assert.That(npvm[nameof(NewPasswordViewModel.Verification)], Is.EqualTo(""));
            Assert.That(npvm.ValidationError, Is.EqualTo((int)ValidationError.None));
        }

        [Test]
        public static void TestValidatePassphraseNotOk()
        {
            NewPasswordViewModel npvm = new NewPasswordViewModel(String.Empty, String.Empty);

            npvm.Password = "abc1234";
            npvm.Verification = "abc12345";

            Assert.That(npvm[nameof(NewPasswordViewModel.Verification)], Is.Not.EqualTo(""));
            Assert.That(npvm.ValidationError, Is.EqualTo((int)ValidationError.VerificationPassphraseWrong));
        }

        [Test]
        public static void TestValidateNonExistingPropertyName()
        {
            NewPasswordViewModel npvm = new NewPasswordViewModel(String.Empty, String.Empty);
            string s = null;
            Assert.Throws<ArgumentException>(() => { s = npvm["NonExisting"]; });
            Assert.That(s, Is.Null, "Not a real assertion, only to make the variable used for FxCop.");
        }

        [TestCase(CryptoImplementation.Mono)]
        [TestCase(CryptoImplementation.WindowsDesktop)]
        [TestCase(CryptoImplementation.BouncyCastle)]
        public static void TestValidateWrongPassphraseWithRealFile(CryptoImplementation cryptoImplementation)
        {
            SetupAssembly.AssemblySetupCrypto(cryptoImplementation);

            FakeDataStore.AddFile(@"C:\My Folder\MyFile-txt.axx", new MemoryStream(Resources.helloworld_key_a_txt));
            NewPasswordViewModel npvm = new NewPasswordViewModel(String.Empty, @"C:\My Folder\MyFile-txt.axx");
            npvm.Password = "b";
            npvm.Verification = "b";

            Assert.That(npvm[nameof(NewPasswordViewModel.Password)], Is.Not.EqualTo(""));
            Assert.That(npvm.ValidationError, Is.EqualTo((int)ValidationError.WrongPassphrase));
        }
    }
}
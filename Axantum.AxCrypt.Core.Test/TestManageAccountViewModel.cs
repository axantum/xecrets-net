using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Service;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestManageAccountViewModel
    {
        private CryptoImplementation _cryptoImplementation;

        public TestManageAccountViewModel(CryptoImplementation cryptoImplementation)
        {
            _cryptoImplementation = cryptoImplementation;
        }

        [SetUp]
        public void Setup()
        {
            SetupAssembly.AssemblySetup();
            SetupAssembly.AssemblySetupCrypto(_cryptoImplementation);
        }

        [TearDown]
        public void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public void TestChangePassword()
        {
            UserKeyPair key1 = new UserKeyPair(EmailAddress.Parse("svante@axantum.com"), 512);
            UserKeyPair key2 = new UserKeyPair(EmailAddress.Parse("svante@axantum.com"), 512);

            var mockUserAsymmetricKeysStore = new Mock<AccountStorage>((IAccountService)null);
            mockUserAsymmetricKeysStore.Setup<IEnumerable<UserKeyPair>>(f => f.AllKeyPairs).Returns(new UserKeyPair[] { key1, key2 });
            string passphraseUsed = String.Empty;
            mockUserAsymmetricKeysStore.Setup(f => f.ChangePassphrase(It.IsAny<Passphrase>()))
                .Callback<Passphrase>((passphrase) =>
                {
                    passphraseUsed = passphrase.Text;
                });

            var mockKnownIdentities = new Mock<KnownIdentities>();

            ManageAccountViewModel viewModel = new ManageAccountViewModel(mockUserAsymmetricKeysStore.Object, mockKnownIdentities.Object);
            IEnumerable<AccountProperties> emailsList = null;
            viewModel.BindPropertyChanged(nameof(ManageAccountViewModel.AccountProperties), (IEnumerable<AccountProperties> emails) => emailsList = emails);
            Assert.That(emailsList.Count(), Is.EqualTo(2), "There should be two accounts now.");
            Assert.That(emailsList.First().EmailAddress, Is.EqualTo("svante@axantum.com"), "The first should be 'svante@axantum.com'");
            Assert.That(emailsList.Last().EmailAddress, Is.EqualTo("svante@axantum.com"), "The last should be 'svante@axantum.com'");

            viewModel.ChangePassphrase.Execute("allan");
            Assert.That(passphraseUsed, Is.EqualTo("allan"));
        }
    }
}
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
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
            UserAsymmetricKeys key1 = new UserAsymmetricKeys(new EmailAddress("svante@axantum.com"), 512);
            UserAsymmetricKeys key2 = new UserAsymmetricKeys(new EmailAddress("svante@axantum.com"), 512);

            var mock = new Mock<UserAsymmetricKeysStore>();
            mock.Setup<IEnumerable<UserAsymmetricKeys>>(f => f.Keys).Returns(new UserAsymmetricKeys[] { key1, key2 });
            mock.Setup<bool>(f => f.HasStore).Returns(true);
            string passphraseUsed = String.Empty;
            mock.Setup(f => f.Save(It.IsAny<Passphrase>()))
                .Callback<Passphrase>((passphrase) =>
                {
                    passphraseUsed = passphrase.Text;
                });

            ManageAccountViewModel viewModel = new ManageAccountViewModel(mock.Object);
            IEnumerable<AccountProperties> emailsList = null;
            viewModel.BindPropertyChanged("AccountEmails", (IEnumerable<AccountProperties> emails) => emailsList = emails);
            Assert.That(emailsList.Count(), Is.EqualTo(2), "There should be two accounts now.");
            Assert.That(emailsList.First().EmailAddress, Is.EqualTo("svante@axantum.com"), "The first should be 'svante@axantum.com'");
            Assert.That(emailsList.Last().EmailAddress, Is.EqualTo("svante@axantum.com"), "The last should be 'svante@axantum.com'");

            viewModel.ChangePassphrase.Execute("allan");
            Assert.That(passphraseUsed, Is.EqualTo("allan"));
        }
    }
}
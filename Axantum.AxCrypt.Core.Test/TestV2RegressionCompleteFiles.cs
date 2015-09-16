using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.UI;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestV2RegressionCompleteFiles
    {
        private static readonly string _rootPath = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

        private CryptoImplementation _cryptoImplementation;

        public TestV2RegressionCompleteFiles(CryptoImplementation cryptoImplementation)
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
        public void TestSimpleSmallFile256()
        {
            TestOneFile("short-txt-AES256.axx", "PâsswördètMëd§½ Lôñg|´¨", "2de4823aa40ed2a6d040e7ba67bf60e3b1ae5c1f1bc2391ba8435ec7d1597f49");
        }

        [Test]
        public void TestLargerUncompressibleFile256()
        {
            TestOneFile("snow-jpg-AES256.axx", "PâsswördètMëd§½ Lôñg|´¨", "b541684642894f9385b15ddd62f980e20a730fc036bcb1bbb4bad75b1f4889b4");
        }

        [Test]
        public void TestLargerCompressibleTextFile256()
        {
            TestOneFile("Frankenstein-txt-AES256.axx", "PâsswördètMëd§½ Lôñg|´¨", "3493994a1a7d891e1a6fb4e3f60c58cbfb3e6f71f12f4c3ffe51c0c9498eb520");
        }

        [Test]
        public void TestSimpleSmallFile128()
        {
            TestV2RegressionCompleteFiles.TestOneFile("short-txt-V2AES128.axx", "PâsswördètMëd§½ Lôñg|´¨", "2de4823aa40ed2a6d040e7ba67bf60e3b1ae5c1f1bc2391ba8435ec7d1597f49");
        }

        [Test]
        public void TestLargerUncompressibleFile128()
        {
            TestV2RegressionCompleteFiles.TestOneFile("snow-jpg-V2AES128.axx", "PâsswördètMëd§½ Lôñg|´¨", "b541684642894f9385b15ddd62f980e20a730fc036bcb1bbb4bad75b1f4889b4");
        }

        [Test]
        public void TestLargerCompressibleTextFile128()
        {
            TestV2RegressionCompleteFiles.TestOneFile("Frankenstein-txt-V2AES128.axx", "PâsswördètMëd§½ Lôñg|´¨", "3493994a1a7d891e1a6fb4e3f60c58cbfb3e6f71f12f4c3ffe51c0c9498eb520");
        }

        internal static void TestOneFile(string resourceName, string password, string sha256HashValue)
        {
            string source = Path.Combine(_rootPath, "source.axx");
            string destination = Path.Combine(_rootPath, "destination.file");
            Stream stream = Assembly.GetAssembly(typeof(TestV2RegressionCompleteFiles)).GetManifestResourceStream("Axantum.AxCrypt.Core.Test.resources." + resourceName);
            FakeDataStore.AddFile(source, FakeDataStore.TestDate1Utc, FakeDataStore.TestDate2Utc, FakeDataStore.TestDate3Utc, stream);

            LogOnIdentity passphrase = new LogOnIdentity(password);

            bool ok = new AxCryptFile().Decrypt(TypeMap.Resolve.New<IDataStore>(source), TypeMap.Resolve.New<IDataStore>(destination), passphrase, AxCryptOptions.SetFileTimes, new ProgressContext());
            Assert.That(ok, Is.True, "The Decrypt() method should return true for ok.");

            byte[] hash;
            HashAlgorithm hashAlgorithm = SHA256.Create();
            Stream plainStream = TypeMap.Resolve.New<IDataStore>(destination).OpenRead();
            using (Stream cryptoStream = new CryptoStream(plainStream, hashAlgorithm, CryptoStreamMode.Read))
            {
                plainStream = null;
                cryptoStream.CopyTo(Stream.Null);
            }
            hash = hashAlgorithm.Hash;

            Assert.That(hash.IsEquivalentTo(sha256HashValue.FromHex()), "Wrong SHA-256.");
        }
    }
}
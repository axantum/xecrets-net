#region Xecrets Cli Copyright and GPL License notice

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

using AxCrypt.Abstractions;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Extensions;
using AxCrypt.Core.IO;
using AxCrypt.Core.UI;
using AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

using static AxCrypt.Abstractions.TypeResolve;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace AxCrypt.Core.Test
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
            SetupAssembly.AssemblySetup(_cryptoImplementation);
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
            Stream stream = Assembly.GetAssembly(typeof(TestV2RegressionCompleteFiles)).GetManifestResourceStream("Xecrets.Net.Core.Test.resources." + resourceName);
            FakeDataStore.AddFile(source, FakeDataStore.TestDate1Utc, FakeDataStore.TestDate2Utc, FakeDataStore.TestDate3Utc, stream);

            LogOnIdentity passphrase = new LogOnIdentity(password);

            bool ok = new AxCryptFile().Decrypt(New<IDataStore>(source), New<IDataStore>(destination), passphrase, AxCryptOptions.SetFileTimes, new ProgressContext());
            Assert.That(ok, Is.True, "The Decrypt() method should return true for ok.");

            byte[] hash;
            HashAlgorithm hashAlgorithm = SHA256.Create();
            Stream plainStream = New<IDataStore>(destination).OpenRead();
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

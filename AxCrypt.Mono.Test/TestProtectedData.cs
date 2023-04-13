#region Coypright and GPL License

/*
 * Xecrets File Core - Copyright 2022, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets File Core, parts of which in turn are derived from AxCrypt as licensed under GPL v3 or later.
 * 
 * However, this code is not derived from AxCrypt and is separately copyrighted and only licensed as follows unless
 * explicitly licensed otherwise. If you use any part of this code in your software, please see https://www.gnu.org/licenses/
 * for details of what this means for you.
 *
 * Xecrets File Core is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets File Core is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets File Core.  If not, see <https://www.gnu.org/licenses/>.
 *
 * The source repository can be found at https://github.com/ please go there for more information, suggestions and
 * contributions. You may also visit https://www.axantum.com for more information about the author.
*/

#endregion Coypright and GPL License

using System.Text;

using AxCrypt.Abstractions;
using AxCrypt.Mono;

using static AxCrypt.Abstractions.TypeResolve;

using NUnit.Framework;
using AxCrypt.Fake;
using AxCrypt.Core.Header;

namespace Xecrets.File.Implementation.NET.Test
{
    [TestFixture]
    internal static class TestProtectedData
    {
        private static readonly byte[] _axCryptGuid = AxCrypt1Guid.GetBytes();

        [SetUp]
        public static void Setup()
        {
            TypeMap.Register.Singleton<IReport>(() => new FakeReport());
            TypeMap.Register.Singleton<IProtectedData>(() => new ProtectedDataImplementation("Test.Xecrets.File.Implementation"));
        }

        [TearDown]
        public static void Teardown()
        {
            TypeMap.Register.Clear();
        }

        [Test]
        public static void TestSimpleEncryptionDecryption()
        {
            var protector = New<IProtectedData>();
            string originalText = "A not so secret string...";
            byte[] plainText = Encoding.UTF8.GetBytes(originalText);
            byte[] cipherText = protector.Protect(plainText, null);
            
            Assert.That(cipherText.Length > plainText.Length, "The encrypted data must be longer and different from the plain text.");
            
            byte[]? recoveredPlainText = protector.Unprotect(cipherText, null);
            string recoveredText = Encoding.UTF8.GetString(recoveredPlainText!);
            
            Assert.That(recoveredText, Is.EqualTo(originalText), "The recovered text should be identical to the original.");
        }

        [Test]
        public static void TestDecryptionOfAnAxCryptFileData()
        {
            var protector = New<IProtectedData>();
            string originalText = "A not so secret string...";
            byte[] plainText = Encoding.UTF8.GetBytes(originalText);

            byte[] notReallyAnAxCryptFileButLooksLikeOneToStartWith = _axCryptGuid.Concat(plainText).ToArray();

            byte[]? attemptedUnprotection = protector.Unprotect(notReallyAnAxCryptFileButLooksLikeOneToStartWith, null);

            Assert.That(attemptedUnprotection, Is.Null, "The unprotection should silently fail, and return null.");

            var FakeReport = (FakeReport)New<IReport>();
            Assert.That(FakeReport.LastReport, Is.Empty, "No exception should occur.");
        }

        [Test]
        public static void TestFailedDecryptionOfUnprotectedData()
        {
            var protector = New<IProtectedData>();
            string originalText = "A not so secret string...";
            byte[] plainText = Encoding.UTF8.GetBytes(originalText);

            byte[]? attemptedUnprotection = protector.Unprotect(plainText, null);

            Assert.That(attemptedUnprotection, Is.Null, "The unprotection should silently fail, and return null.");

            var FakeReport = (FakeReport)New<IReport>();
            Assert.That(FakeReport.LastReport, Is.Not.Empty, "An exception should have occurred.");
        }

        [Test]
        public static void TestFailedDecryptionOfSlightlyMangledData()
        {
            var protector = New<IProtectedData>();
            string originalText = "A not so secret string...";
            byte[] plainText = Encoding.UTF8.GetBytes(originalText);
            byte[] cipherText = protector.Protect(plainText, null);

            Assert.That(cipherText.Length > plainText.Length, "The encrypted data must be longer and different from the plain text.");

            cipherText[0] += 1;

            byte[]? attemptedUnprotection = protector.Unprotect(cipherText, null);

            Assert.That(attemptedUnprotection, Is.Null, "The unprotection should silently fail, and return null.");

            var FakeReport = (FakeReport)New<IReport>();
            Assert.That(FakeReport.LastReport, Is.Not.Empty, "An exception should have occurred.");
        }
    }
}

#region Coypright and License

/*
 * AxCrypt - Copyright 2015, Svante Seleborg, All Rights Reserved
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

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Core.Algorithm;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Service;
using Axantum.AxCrypt.Core.UI;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestExtensions
    {
        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup();
        }

        [TearDown]
        public static void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public static void TestStringInvariantFormat()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("sv-SE");
            DateTime dateTime = new DateTime(2001, 2, 3, 4, 5, 6, DateTimeKind.Utc);

            string dateTimeSwedishFormatted = String.Format(CultureInfo.CurrentCulture, "Formatted: {0}", dateTime);
            Assert.That(dateTimeSwedishFormatted, Is.EqualTo("Formatted: 2001-02-03 04:05:06"));

            string dateTimeInvariantFormatted = "Formatted: {0}".InvariantFormat(dateTime);
            Assert.That(dateTimeInvariantFormatted, Is.EqualTo("Formatted: 02/03/2001 04:05:06"));
        }

        [Test]
        public static void TestStringFormatWith()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("sv-SE");
            DateTime dateTime = new DateTime(2001, 2, 3, 4, 5, 6, DateTimeKind.Utc);

            string dateTimeSwedishFormatted = "Formatted: {0}".FormatWith(CultureInfo.CurrentCulture, dateTime);
            Assert.That(dateTimeSwedishFormatted, Is.EqualTo("Formatted: 2001-02-03 04:05:06"));

            string dateTimeInvariantFormatted = "Formatted: {0}".FormatWith(CultureInfo.InvariantCulture, dateTime);
            Assert.That(dateTimeInvariantFormatted, Is.EqualTo("Formatted: 02/03/2001 04:05:06"));
        }

        [Test]
        public static void TestByteArrayLocate()
        {
            byte[] testArray = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x09, 0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01, 0x00 };

            int locationSingleByteFirstInArray = testArray.Locate(new byte[] { 0x01 }, 0, testArray.Length);
            Assert.That(locationSingleByteFirstInArray, Is.EqualTo(0), "Looking for a single byte that is first in the array.");

            int locationSingleByteLastInArray = testArray.Locate(new byte[] { 0x00 }, 0, testArray.Length);
            Assert.That(locationSingleByteLastInArray, Is.EqualTo(testArray.Length - 1), "Looking for a single byte that is last in the array.");

            int locationSingleByteNotInArray = testArray.Locate(new byte[] { 0xff }, 0, testArray.Length);
            Assert.That(locationSingleByteNotInArray, Is.EqualTo(-1), "Looking for a single byte that is not part of the array.");

            int locationSingleByteNotInPartOfArray = testArray.Locate(new byte[] { 0x01 }, 1, testArray.Length - 3);
            Assert.That(locationSingleByteNotInPartOfArray, Is.EqualTo(-1), "Looking for a single byte that is not in a selected part of the array.");

            int locationMultipleBytesFirstInArray = testArray.Locate(new byte[] { 0x01, 0x02, 0x03 }, 0, testArray.Length);
            Assert.That(locationMultipleBytesFirstInArray, Is.EqualTo(0), "Looking for multiple bytes that are first in the array.");

            int locationMultipleBytesLastInArray = testArray.Locate(new byte[] { 0x02, 0x01, 0x00 }, 0, testArray.Length);
            Assert.That(locationMultipleBytesLastInArray, Is.EqualTo(testArray.Length - 3), "Looking for multiple bytes that are last in the array.");

            int locationMultipleBytesMiddleInArray = testArray.Locate(new byte[] { 0x09, 0x09, 0x08 }, 0, testArray.Length);
            Assert.That(locationMultipleBytesMiddleInArray, Is.EqualTo(8), "Looking for multiple bytes that are in the middle of the array.");

            int locationMultipleBytesNotInArray = testArray.Locate(new byte[] { 0x02, 0x00, 0x01 }, 0, testArray.Length);
            Assert.That(locationMultipleBytesNotInArray, Is.EqualTo(-1), "Looking for multiple bytes that are not in the array.");

            int locationMultipleBytesLastInPartOfArray = testArray.Locate(new byte[] { 0x03, 0x02, 0x01 }, 2, testArray.Length - 3);
            Assert.That(locationMultipleBytesLastInPartOfArray, Is.EqualTo(testArray.Length - 4), "Looking for multiple bytes that are last in part of the array.");

            int locationMultipleBytesNotInPartOfArray = testArray.Locate(new byte[] { 0x03, 0x02, 0x01 }, 2, testArray.Length - 4);
            Assert.That(locationMultipleBytesNotInPartOfArray, Is.EqualTo(-1), "Looking for multiple bytes that are not in part of the array.");
        }

        [Test]
        public static void TestByteArrayXor()
        {
            byte[] testArray = new byte[] { 0x01, 0x02, 0x04, 0x08, 0x00, 0xff };

            byte[] nullArray = null;

            TestDelegate bufferArgumentNullException = () => nullArray.Xor(testArray);
            Assert.Throws<ArgumentNullException>(bufferArgumentNullException, "Calling with a null-reference buffer.");

            TestDelegate otherArgumentNullException = () => testArray.Xor(nullArray);
            Assert.Throws<ArgumentNullException>(otherArgumentNullException, "Calling with a null-reference other argument.");
            Assert.That(testArray, Is.EqualTo(new byte[] { 0x01, 0x02, 0x04, 0x08, 0x00, 0xff }), "Assuring that the array is still unmodified.");

            byte[] xorArray = (byte[])testArray.Clone();
            xorArray.Xor(testArray);
            Assert.That(xorArray, Is.EqualTo(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }), "Xor with all of itself should yield all zeros.");

            xorArray.Xor(new byte[] { 0x01, 0x02, 0x04 });
            Assert.That(xorArray, Is.EqualTo(new byte[] { 0x01, 0x02, 0x04, 0x00, 0x00, 0x00 }), "Xor with a short array should only affect those bytes.");

            xorArray.Xor(new byte[] { 0x00, 0x00, 0x00, 0x08, 0x00, 0xff, 0x12, 0x34, 0x56 });
            Assert.That(xorArray, Is.EqualTo(testArray), "Xor with a longer array should just ignore the extra bytes in other.");

            TestDelegate bufferArgumentWithAllParametersNullException = () => nullArray.Xor(0, testArray, 0, testArray.Length);
            Assert.Throws<ArgumentNullException>(bufferArgumentWithAllParametersNullException, "Calling all parameters overload with null-reference buffer.");

            TestDelegate otherArgumentWithAllParametersNullException = () => testArray.Xor(0, nullArray, 0, testArray.Length);
            Assert.Throws<ArgumentNullException>(otherArgumentWithAllParametersNullException, "Calling all parameters overload with a null-reference other argument.");
            Assert.That(testArray, Is.EqualTo(new byte[] { 0x01, 0x02, 0x04, 0x08, 0x00, 0xff }), "Assuring that the array is still unmodified.");

            TestDelegate bufferLengthArgumentOutOfRangeException = () => testArray.Xor(1, testArray, 0, testArray.Length);
            Assert.Throws<ArgumentOutOfRangeException>(bufferLengthArgumentOutOfRangeException, "Calling with an offset into buffer causing length to be too long.");
            Assert.That(testArray, Is.EqualTo(new byte[] { 0x01, 0x02, 0x04, 0x08, 0x00, 0xff }), "Assuring that the array is still unmodified.");

            TestDelegate otherLengthArgumentOutOfRangeException = () => testArray.Xor(0, testArray, 1, testArray.Length);
            Assert.Throws<ArgumentOutOfRangeException>(otherLengthArgumentOutOfRangeException, "Calling with an offset into other causing length to be too long.");
            Assert.That(testArray, Is.EqualTo(new byte[] { 0x01, 0x02, 0x04, 0x08, 0x00, 0xff }), "Assuring that the array is still unmodified.");

            xorArray.Xor(2, testArray, 4, 2);
            Assert.That(xorArray, Is.EqualTo(new byte[] { 0x01, 0x02, 0x04, 0xf7, 0x00, 0xff }), "Xor part of the buffer with part of the other byte array.");

            Assert.That(xorArray, Is.Not.EqualTo(new byte[] { 0x01, 0x02, 0x04, 0xf7, 0xff, 0xff }), "Sanity check to ensure that we're really testing what we think.");
        }

        [Test]
        public static void TestByteArrayAppend()
        {
            byte[] one = new byte[] { 0x01 };
            byte[] two = new byte[] { 0x02, 0x02 };
            byte[] three = new byte[] { 0x03, 0x03, 0x03 };

            byte[] noAppend = one.Append();
            Assert.That(noAppend, Is.EqualTo(one), "Append with zero arrays should yield the same.");
            Assert.IsFalse(Object.ReferenceEquals(noAppend, one), "Append with zero arrays should yield a copy, not the same instance.");

            byte[] oneTwoAppend = one.Append(two);
            Assert.That(oneTwoAppend, Is.EqualTo(new byte[] { 0x01, 0x02, 0x02 }), "Append of one with two should yield the appended result.");

            byte[] oneTwoThreeAppend = one.Append(two, three);
            Assert.That(oneTwoThreeAppend, Is.EqualTo(new byte[] { 0x01, 0x02, 0x02, 0x03, 0x03, 0x03 }), "Append of one with two and three should yield the appended result.");

            byte[] oneTwoAppendThreeAppend = one.Append(two).Append(three);
            Assert.That(oneTwoAppendThreeAppend, Is.EqualTo(new byte[] { 0x01, 0x02, 0x02, 0x03, 0x03, 0x03 }), "Append of one with two and append again three should yield the appended result (just less efficiently).");
        }

        [Test]
        public static void TestByteArrayIsEquivalentTo()
        {
            byte[] one = new byte[] { 0x01 };
            byte[] two = new byte[] { 0x02, 0x01 };
            byte[] three = new byte[] { 0x02, 0x01, 0x03 };
            byte[] nullArray = null;
            bool isEquivalent = false;

            Assert.Throws<ArgumentNullException>(() => { isEquivalent = nullArray.IsEquivalentTo(0, one, 0, 1); });
            Assert.Throws<ArgumentNullException>(() => { isEquivalent = one.IsEquivalentTo(0, nullArray, 0, 1); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { isEquivalent = one.IsEquivalentTo(one.Length, one, 0, one.Length); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { isEquivalent = two.IsEquivalentTo(0, one, 1, one.Length); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { isEquivalent = three.IsEquivalentTo(0, two, 0, two.Length + 1); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { isEquivalent = three.IsEquivalentTo(-1, three, 0, three.Length); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { isEquivalent = two.IsEquivalentTo(0, three, -1, two.Length); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { isEquivalent = one.IsEquivalentTo(0, one, 0, -1); });

            isEquivalent = one.IsEquivalentTo(0, one, 0, 1);
            Assert.IsTrue(isEquivalent, "An array should be equivalent to itself.");

            isEquivalent = one.IsEquivalentTo(0, two, 1, 1);
            Assert.IsTrue(isEquivalent, "'one' should be equivalent to second byte of 'two'.");

            isEquivalent = three.IsEquivalentTo(1, one, 0, 1);
            Assert.IsTrue(isEquivalent, "the second byte of 'three' should be equivalent to 'one'");

            isEquivalent = two.IsEquivalentTo(0, three, 1, two.Length);
            Assert.IsFalse(isEquivalent, "'two' should not be equivalent to 'three' with offset one.");

            Assert.Throws<ArgumentNullException>(() => { isEquivalent = nullArray.IsEquivalentTo(one); });
            Assert.Throws<ArgumentNullException>(() => { isEquivalent = one.IsEquivalentTo(nullArray); });

            byte[] threeAgain = new byte[] { 0x02, 0x01, 0x03 };
            isEquivalent = threeAgain.IsEquivalentTo(three);
            Assert.IsTrue(isEquivalent, "'threeAgain' should be equivalent to 'three'.");

            isEquivalent = threeAgain.IsEquivalentTo(two);
            Assert.IsFalse(isEquivalent, "'threeAgain' should be not be equivalent to 'two'.");
        }

        [Test]
        public static void TestEndianOptimization()
        {
            TypeMap.Register.Singleton<IRuntimeEnvironment>(() => new FakeRuntimeEnvironment(Endian.Reverse));
            if (BitConverter.IsLittleEndian)
            {
                byte[] actuallyLittleEndianBytes = 0x0102030405060708L.GetBigEndianBytes();
                Assert.That(actuallyLittleEndianBytes, Is.EqualTo(new byte[] { 0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01 }), "Getting big endian long, thinking we are big endian but in fact are not, will get us little endian bytes.");

                byte[] actuallyIntLittleEndianBytes = 0x01020304.GetBigEndianBytes();
                Assert.That(actuallyIntLittleEndianBytes, Is.EqualTo(new byte[] { 0x04, 0x03, 0x02, 0x01 }), "Getting big endian int, thinking we are big endian but in fact are not, will get us little endian bytes.");

                byte[] actuallyStillLittleEndianBytes = 0x0102030405060708L.GetLittleEndianBytes();
                Assert.That(actuallyStillLittleEndianBytes, Is.EqualTo(new byte[] { 0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01 }), "Getting little endian long, thinking we are big endian but in fact are not, will still get us little endian.");

                byte[] actuallyIntStillLittleEndianBytes = 0x01020304.GetLittleEndianBytes();
                Assert.That(actuallyIntStillLittleEndianBytes, Is.EqualTo(new byte[] { 0x04, 0x03, 0x02, 0x01 }), "Getting little endian int, thinking we are big endian but in fact are not, will still get us little endian.");
            }
            else
            {
                byte[] actuallyStillBigEndianBytes = 0x0102030405060708L.GetBigEndianBytes();
                Assert.That(actuallyStillBigEndianBytes, Is.EqualTo(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 }), "Getting big endian long, thinking we are little endian but in fact are not, will still get us big endian bytes.");

                byte[] actuallyIntStillBigEndianBytes = 0x01020304.GetBigEndianBytes();
                Assert.That(actuallyIntStillBigEndianBytes, Is.EqualTo(new byte[] { 0x01, 0x02, 0x03, 0x04 }), "Getting big endian int, thinking we are little endian but in fact are not, will still get us big endian bytes.");

                byte[] actuallyBigEndianBytes = 0x0102030405060708L.GetLittleEndianBytes();
                Assert.That(actuallyBigEndianBytes, Is.EqualTo(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 }), "Getting little endian long, thinking we are big endian but in fact are not, will get us big endian bytes.");

                byte[] actuallyIntBigEndianBytes = 0x01020304.GetLittleEndianBytes();
                Assert.That(actuallyIntBigEndianBytes, Is.EqualTo(new byte[] { 0x01, 0x02, 0x03, 0x04 }), "Getting little endian int, thinking we are big endian but in fact are not, will get us big endian bytes.");
            }
        }

        [Test]
        public static void TestEndianConversion()
        {
            byte[] actuallyLittleEndianBytes = 0x0102030405060708L.GetBigEndianBytes();
            Assert.That(actuallyLittleEndianBytes, Is.EqualTo(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 }), "Getting big endian long.");

            byte[] actuallyStillLittleEndianBytes = 0x0102030405060708L.GetLittleEndianBytes();
            Assert.That(actuallyStillLittleEndianBytes, Is.EqualTo(new byte[] { 0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01 }), "Getting little endian long.");

            byte[] actuallyIntStillLittleEndianBytes = 0x01020304.GetLittleEndianBytes();
            Assert.That(actuallyIntStillLittleEndianBytes, Is.EqualTo(new byte[] { 0x04, 0x03, 0x02, 0x01 }), "Getting little endian int.");
        }

        [Test]
        public static void TestCreateEncryptedName()
        {
            string rootPath = Path.GetPathRoot(Environment.CurrentDirectory);
            string fileName = rootPath.PathCombine("Users", "Axantum", "A Documents Folder", "My Document.docx");
            string encryptedFileName = TypeMap.Resolve.New<IDataStore>(fileName).CreateEncryptedName().FullName;
            Assert.That(encryptedFileName, Is.EqualTo(rootPath.PathCombine("Users", "Axantum", "A Documents Folder", "My Document-docx.axx")), "Standard conversion of file name to encrypted form.");

            Assert.Throws<InternalErrorException>(() =>
                 {
                     string encryptedEncryptedFileName = TypeMap.Resolve.New<IDataStore>(encryptedFileName).CreateEncryptedName().FullName;

                     // Use the instance to avoid FxCop errors.
                     Object.Equals(encryptedEncryptedFileName, null);
                 });

            fileName = rootPath.PathCombine("Users", "Axantum", "A Documents Folder", "My Extensionless File");
            encryptedFileName = TypeMap.Resolve.New<IDataStore>(fileName).CreateEncryptedName().FullName;
            Assert.That(encryptedFileName, Is.EqualTo(rootPath.PathCombine("Users", "Axantum", "A Documents Folder", "My Extensionless File.axx")), "Conversion of file name without extension to encrypted form.");

            Assert.Throws<InternalErrorException>(() =>
            {
                string encryptedEncryptedFileName = TypeMap.Resolve.New<IDataStore>(encryptedFileName).CreateEncryptedName().FullName;

                // Use the instance to avoid FxCop errors.
                Object.Equals(encryptedEncryptedFileName, null);
            });
        }

        [Test]
        public static void TestTrimLogMessage()
        {
            string trimmed, untrimmed;

            untrimmed = "This text should not be trimmed.";
            trimmed = untrimmed.TrimLogMessage();
            Assert.That(trimmed, Is.EqualTo(untrimmed), "The text does not contain anything that should be trimmed.");

            untrimmed = "Information: Should not be trimmed.";
            trimmed = untrimmed.TrimLogMessage();
            Assert.That(trimmed, Is.EqualTo(untrimmed), "The text does not contain anything that should be trimmed.");

            untrimmed = "blah blah Warning - should be trimmed!";
            trimmed = untrimmed.TrimLogMessage();
            Assert.That(trimmed, Is.EqualTo("Warning - should be trimmed!"), "The text should be trimmed at the start.");

            untrimmed = " Warning - should be trimmed!";
            trimmed = untrimmed.TrimLogMessage();
            Assert.That(trimmed, Is.EqualTo("Warning - should be trimmed!"), "The text should be trimmed at the start, even just a single space.");

            untrimmed = "18:30 Debug - should be trimmed!";
            trimmed = untrimmed.TrimLogMessage();
            Assert.That(trimmed, Is.EqualTo("Debug - should be trimmed!"), "The text should be trimmed at the start.");

            untrimmed = "Information Error Error - should be trimmed!";
            trimmed = untrimmed.TrimLogMessage();
            Assert.That(trimmed, Is.EqualTo("Error Error - should be trimmed!"), "The text should be trimmed at the start, but only to the first occurrence.");

            untrimmed = "Prefix Fatal";
            trimmed = untrimmed.TrimLogMessage();
            Assert.That(trimmed, Is.EqualTo("Fatal"), "The text should be trimmed at the start.");
        }

        [Test]
        public static void TestCopyToArgumentExceptions()
        {
            Stream nullStream = null;
            using (Stream stream = new MemoryStream())
            {
                Assert.Throws<ArgumentNullException>(() => { StreamExtensions.CopyTo(nullStream, stream, 100); });
                Assert.Throws<ArgumentNullException>(() => { StreamExtensions.CopyTo(stream, nullStream, 100); });
                Assert.Throws<ArgumentOutOfRangeException>(() => { StreamExtensions.CopyTo(stream, stream, 0); });
                Assert.Throws<ArgumentOutOfRangeException>(() => { StreamExtensions.CopyTo(stream, stream, -1); });
            }
        }

        [Test]
        public static void TestCreateUniqueFileFirstIsOk()
        {
            using (FileLock lockedUnique = @"C:\temp\test.txt".CreateUniqueFile())
            {
                Assert.That(lockedUnique.DataStore.FullName, Is.EqualTo(@"C:\temp\test.txt".NormalizeFilePath()));
            }
        }

        [Test]
        public static void TestCreateUniqueFileFirstIsNotOk()
        {
            IDataStore fileInfo = TypeMap.Resolve.New<IDataStore>(@"C:\temp\test.txt");
            using (Stream stream = fileInfo.OpenWrite())
            {
            }
            using (FileLock lockedUnique = @"C:\temp\test.txt".CreateUniqueFile())
            {
                Assert.That(lockedUnique.DataStore.FullName, Is.EqualTo(@"C:\temp\test.1.txt".NormalizeFilePath()));
            }
        }

        [Test]
        public static void TestCreateUniqueFileReallyCreates()
        {
            using (FileLock lockedUnique = @"C:\temp\test.txt".CreateUniqueFile())
            {
                Assert.That(lockedUnique.DataStore.FullName, Is.EqualTo(@"C:\temp\test.txt".NormalizeFilePath()));
            }

            using (FileLock lockedUnique = @"C:\temp\test.txt".CreateUniqueFile())
            {
                Assert.That(lockedUnique.DataStore.FullName, Is.EqualTo(@"C:\temp\test.1.txt".NormalizeFilePath()));
            }
        }

        [Test]
        public static void TestCreateUniqueFileUnexpectedAxCryptException()
        {
            EventHandler handler = delegate (object sender, EventArgs e)
            {
                if (((FakeDataStore)sender).TestTag == "CreateNewFile")
                {
                    throw new InternalErrorException("An unexpected exception.", ErrorStatus.InternalError);
                }
            };
            FakeDataStore.ExceptionHook += handler;
            try
            {
                Assert.Throws<InternalErrorException>(() => @"C:\temp\test.txt".CreateUniqueFile());
            }
            finally
            {
                FakeDataStore.ExceptionHook -= handler;
            }
        }

        [Test]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "TestIs")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Encryptable")]
        public static void TestIsEncryptable()
        {
            OS.PathFilters.Add(new Regex(@"^C:\{0}Windows\{0}(?!Temp$)".InvariantFormat(Path.DirectorySeparatorChar)));

            Assert.That(TypeMap.Resolve.New<IDataStore>(@"C:\Temp\test.txt").IsEncryptable(), Is.True);
            Assert.That(TypeMap.Resolve.New<IDataStore>(@"C:\Windows\test.txt").IsEncryptable(), Is.False);
            Assert.That(TypeMap.Resolve.New<IDataStore>(@"C:\Temp\test-txt.axx").IsEncryptable(), Is.False);

            IDataStore nullFileInfo = null;
            Assert.Throws<ArgumentNullException>(() => nullFileInfo.IsEncryptable());
        }

        [Test]
        public static void TestNormalizeFolder()
        {
            string expected = @"C:\Documents\".Replace('\\', Path.DirectorySeparatorChar);
            Assert.That(TypeMap.Resolve.New<IDataContainer>(@"C:\Documents\").FullName, Is.EqualTo(expected));
            Assert.That(TypeMap.Resolve.New<IDataContainer>(@"C:/Documents\").FullName, Is.EqualTo(expected));
            Assert.That(TypeMap.Resolve.New<IDataContainer>(@"C:\Documents").FullName, Is.EqualTo(expected));
            Assert.That(TypeMap.Resolve.New<IDataContainer>(@"C:\Documents\\//").FullName, Is.EqualTo(expected));
        }

        [Test]
        public static void TestFolderFromEnvironment()
        {
            string nullString = null;
            Assert.Throws<ArgumentNullException>(() => nullString.FolderFromEnvironment());

            string expected = @"C:\Windows\".Replace('\\', Path.DirectorySeparatorChar);

            SetupAssembly.FakeRuntimeEnvironment.EnvironmentVariables.Add("VARIABLE", expected);

            Assert.That("VARIABLE".FolderFromEnvironment().FullName, Is.EqualTo(expected));
            Assert.That("UNKNOWN".FolderFromEnvironment(), Is.Null);
        }

        [Test]
        public static void TestFallbackExtension()
        {
            Assert.That(default(int).Fallback<int>(2), Is.EqualTo(2));
            Assert.That(3.Fallback<int>(3), Is.EqualTo(3));
        }

        [Test]
        public static void TestFileInfoTypeExtension()
        {
            FakeDataStore.AddFile(@"c:\test.txt", null);
            IDataStore fileInfo = TypeMap.Resolve.New<IDataStore>(@"c:\test.txt");
            Assert.That(fileInfo.Type(), Is.EqualTo(FileInfoTypes.EncryptableFile));

            FakeDataStore.AddFile(@"c:\test-txt.axx", null);
            fileInfo = TypeMap.Resolve.New<IDataStore>(@"c:\test-txt.axx");
            Assert.That(fileInfo.Type(), Is.EqualTo(FileInfoTypes.EncryptedFile));

            FakeDataStore.AddFolder(@"c:\test\");
            IDataContainer folderInfo = TypeMap.Resolve.New<IDataContainer>(@"c:\test\");
            Assert.That(folderInfo.Type(), Is.EqualTo(FileInfoTypes.Folder));

            fileInfo = TypeMap.Resolve.New<IDataStore>(@"c:\not-there.txt");
            Assert.That(fileInfo.Type(), Is.EqualTo(FileInfoTypes.NonExisting));

            OS.PathFilters.Add(new Regex(@"^C:\{0}Windows\{0}".InvariantFormat(Path.DirectorySeparatorChar)));
            FakeDataStore.AddFile(@"C:\Windows\System.drv", null);
            fileInfo = TypeMap.Resolve.New<IDataStore>(@"C:\Windows\System.drv");
            Assert.That(fileInfo.Type(), Is.EqualTo(FileInfoTypes.OtherFile));
        }

        [Test]
        public static void TestReduceByteArrayTooShort()
        {
            byte[] big = new byte[5];
            byte[] reduced = null;

            Assert.Throws<ArgumentException>(() => reduced = big.Reduce(6));
            Assert.That(reduced, Is.Null);
        }

        [Test]
        public static void TestFromHexBadArguments()
        {
            string nullString = null;
            Assert.Throws<ArgumentNullException>(() => nullString.FromHex());

            Assert.Throws<ArgumentException>(() => "01A".FromHex());
        }

        [Test]
        public static void TestCopyToWithBufferSize()
        {
            using (Stream input = new MemoryStream())
            {
                input.Write(new byte[] { 1, 2, 3, 4 }, 0, 4);
                input.Position = 0;
                using (MemoryStream output = new MemoryStream())
                {
                    StreamExtensions.CopyTo(input, output, 2);

                    byte[] copy = output.ToArray();
                    Assert.That(copy.IsEquivalentTo(new byte[] { 1, 2, 3, 4 }));
                }
            }
        }

        [TestCase(CryptoImplementation.Mono)]
        [TestCase(CryptoImplementation.WindowsDesktop)]
        [TestCase(CryptoImplementation.BouncyCastle)]
        public static void TestDecryptToBadArgumentsCausingEarlyException(CryptoImplementation cryptoImplementation)
        {
            SetupAssembly.AssemblySetupCrypto(cryptoImplementation);

            Stream nullStream = null;
            ICryptoTransform nullEncryptor = null;
            ICryptoTransform encryptor = new V2AesCrypto(SymmetricKey.Zero256, SymmetricIV.Zero128, 0).DecryptingTransform();

            Assert.Throws<ArgumentNullException>(() => nullStream.DecryptTo(Stream.Null, encryptor, true));
            Assert.Throws<ArgumentNullException>(() => Stream.Null.DecryptTo(nullStream, encryptor, true));
            Assert.Throws<ArgumentNullException>(() => Stream.Null.DecryptTo(Stream.Null, nullEncryptor, true));
        }

        [TestCase(CryptoImplementation.Mono)]
        [TestCase(CryptoImplementation.WindowsDesktop)]
        [TestCase(CryptoImplementation.BouncyCastle)]
        public static void TestUserAsymmetricKeysToAccountKeyAndBack(CryptoImplementation cryptoImplementation)
        {
            SetupAssembly.AssemblySetupCrypto(cryptoImplementation);

            UserKeyPair originalKeys = new UserKeyPair(EmailAddress.Parse("svante@axcrypt.net"), 512);
            AccountKey accountKey = originalKeys.ToAccountKey(new Passphrase("password"));
            UserKeyPair roundtripKeys = accountKey.ToUserAsymmetricKeys(new Passphrase("password"));

            Assert.That(originalKeys, Is.EqualTo(roundtripKeys));
        }

        [TestCase(CryptoImplementation.Mono)]
        [TestCase(CryptoImplementation.WindowsDesktop)]
        [TestCase(CryptoImplementation.BouncyCastle)]
        public static void TestAccountKeyToUserAsymmetricKeysWithWrongPassphrase(CryptoImplementation cryptoImplementation)
        {
            SetupAssembly.AssemblySetupCrypto(cryptoImplementation);

            UserKeyPair originalKeys = new UserKeyPair(EmailAddress.Parse("svante@axcrypt.net"), 512);
            AccountKey accountKey = originalKeys.ToAccountKey(new Passphrase("password"));
            UserKeyPair roundtripKeys = accountKey.ToUserAsymmetricKeys(new Passphrase("wrong password"));

            Assert.That(roundtripKeys, Is.Null);
        }

        [TestCase(CryptoImplementation.Mono)]
        [TestCase(CryptoImplementation.WindowsDesktop)]
        [TestCase(CryptoImplementation.BouncyCastle)]
        public static void TestAccountKeyToUserAsymmericKeysWithOnlyPublicKey(CryptoImplementation cryptoImplementation)
        {
            SetupAssembly.AssemblySetupCrypto(cryptoImplementation);

            UserKeyPair originalKeys = new UserKeyPair(EmailAddress.Parse("svante@axcrypt.net"), 512);
            IAsymmetricKeyPair partialKeyPair = Resolve.AsymmetricFactory.CreateKeyPair(originalKeys.KeyPair.PublicKey.ToString(), String.Empty);
            UserKeyPair originalPartialKeys = new UserKeyPair(originalKeys.UserEmail, originalKeys.Timestamp, partialKeyPair);

            AccountKey accountKey = originalPartialKeys.ToAccountKey(Passphrase.Empty);
            UserKeyPair roundtripKeys = accountKey.ToUserAsymmetricKeys(Passphrase.Empty);

            Assert.That(roundtripKeys, Is.EqualTo(originalPartialKeys));
        }
    }
}
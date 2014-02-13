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
using Axantum.AxCrypt.Core.Header;
using NUnit.Framework;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestV2DocumentHeaders
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
        public static void TestFileTimes()
        {
            V2DocumentHeaders headers = new V2DocumentHeaders(new V2AesCrypto(new AesKey(256)), 10);

            DateTime now = DateTime.UtcNow;
            headers.LastAccessTimeUtc = now;
            headers.LastWriteTimeUtc = now.AddHours(1);
            headers.CreationTimeUtc = now.AddHours(2);

            Assert.That(headers.LastAccessTimeUtc, Is.EqualTo(now));
            Assert.That(headers.LastWriteTimeUtc, Is.EqualTo(now.AddHours(1)));
            Assert.That(headers.CreationTimeUtc, Is.EqualTo(now.AddHours(2)));
        }

        [Test]
        public static void TestCompression()
        {
            V2DocumentHeaders headers = new V2DocumentHeaders(new V2AesCrypto(new AesKey(256)), 10);

            headers.IsCompressed = true;
            Assert.That(headers.IsCompressed, Is.True);

            headers.IsCompressed = false;
            Assert.That(headers.IsCompressed, Is.False);
        }

        [Test]
        public static void TestUnicodeFileNameShort()
        {
            V2DocumentHeaders headers = new V2DocumentHeaders(new V2AesCrypto(new AesKey(256)), 10);

            headers.FileName = "My Secret Document.txt";
            Assert.That(headers.FileName, Is.EqualTo("My Secret Document.txt"));
        }

        [Test]
        public static void TestUnicodeFileNameLong()
        {
            V2DocumentHeaders headers = new V2DocumentHeaders(new V2AesCrypto(new AesKey(256)), 10);

            string longName = "When in the Course of human events, it becomes necessary for one people to dissolve the political bands which have connected them with another, and to assume among the powers of the earth, the separate and equal station to which the Laws of Nature and of Nature's God entitle them, a decent respect to the opinions of mankind requires that they should declare the causes which impel them to the separation.";
            Assert.That(longName.Length, Is.GreaterThan(256));

            headers.FileName = longName;
            Assert.That(headers.FileName, Is.EqualTo(longName));
        }
    }
}
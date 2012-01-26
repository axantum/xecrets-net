#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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
 * The source is maintained at http://AxCrypt.codeplex.com/ please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Axantum.AxCrypt.Core.Header;
using Axantum.AxCrypt.Core.Reader;
using Axantum.AxCrypt.Core.Test.Properties;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public class TestAxCryptReaderMagicGuid
    {
        [SetUp]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This is test, readability and coding ease is a concern, not performance.")]
        public void Setup()
        {
        }

        [Test]
        public static void TestFindMagicGuidFirstAndOnly()
        {
            using (MemoryStream testStream = new MemoryStream())
            {
                AxCrypt1Guid.Write(testStream);
                testStream.Position = 0;
                using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream))
                {
                    Assert.That(axCryptReader.Read(), Is.True, "We should be able to read the Guid");
                    Assert.That(axCryptReader.ItemType, Is.EqualTo(AxCryptItemType.MagicGuid), "We're expecting to have found a MagicGuid");
                }
            }
        }

        [Test]
        public static void TestFindMagicGuidFirstWithMore()
        {
            using (MemoryStream testStream = new MemoryStream())
            {
                AxCrypt1Guid.Write(testStream);
                byte[] someBytes = Encoding.UTF8.GetBytes("This is a test string that we'll convert into some random bytes....");
                testStream.Write(someBytes, 0, someBytes.Length);
                testStream.Position = 0;
                using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream))
                {
                    Assert.That(axCryptReader.Read(), Is.True, "We should be able to read the Guid");
                    Assert.That(axCryptReader.ItemType, Is.EqualTo(AxCryptItemType.MagicGuid), "We're expecting to have found a MagicGuid");
                }
            }
        }

        [Test]
        public static void TestFindMagicGuidWithOtherFirstButNoMore()
        {
            using (MemoryStream testStream = new MemoryStream())
            {
                byte[] someBytes = Encoding.UTF8.GetBytes("This is a test string that we'll convert into some random bytes....");
                testStream.Write(someBytes, 0, someBytes.Length);
                AxCrypt1Guid.Write(testStream);
                testStream.Position = 0;
                using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream))
                {
                    Assert.That(axCryptReader.Read(), Is.True, "We should be able to read the Guid");
                    Assert.That(axCryptReader.ItemType, Is.EqualTo(AxCryptItemType.MagicGuid), "We're expecting to have found a MagicGuid");
                }
            }
        }

        [Test]
        public static void TestFindMagicGuidWithOtherFirstAndMore()
        {
            using (MemoryStream testStream = new MemoryStream())
            {
                byte[] someBytes = Encoding.UTF8.GetBytes("This is a test string that we'll convert into some random bytes....");
                testStream.Write(someBytes, 0, someBytes.Length);
                AxCrypt1Guid.Write(testStream);
                testStream.Write(someBytes, 0, someBytes.Length);
                testStream.Position = 0;
                using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream))
                {
                    Assert.That(axCryptReader.Read(), Is.True, "We should be able to read the Guid");
                    Assert.That(axCryptReader.ItemType, Is.EqualTo(AxCryptItemType.MagicGuid), "We're expecting to have found a MagicGuid");
                }
            }
        }

        [Test]
        public static void TestFindMagicGuidFromSimpleFile()
        {
            using (Stream testStream = new MemoryStream(Resources.HelloWorld_Key_a_txt))
            {
                using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream))
                {
                    Assert.That(axCryptReader.Read(), Is.True, "We should be able to read the Guid");
                    Assert.That(axCryptReader.ItemType, Is.EqualTo(AxCryptItemType.MagicGuid), "We're expecting to have found a MagicGuid");
                }
            }
        }
    }
}
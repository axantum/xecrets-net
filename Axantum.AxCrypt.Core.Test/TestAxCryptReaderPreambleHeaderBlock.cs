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
using Axantum.AxCrypt.Core.Header;
using Axantum.AxCrypt.Core.Reader;
using Axantum.AxCrypt.Core.Test.Properties;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public class TestAxCryptReaderPreambleHeaderBlock
    {
        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This is test, readability and coding ease is a concern, not performance.")]
        public void TestFindPreambleHeaderBlockFirstButMoreThanOnceShouldThrow()
        {
            using (MemoryStream testStream = new MemoryStream())
            {
                AxCrypt1Guid.Write(testStream);
                PreambleHeaderBlock preambleHeaderBlock = new PreambleHeaderBlock();
                preambleHeaderBlock.Write(testStream);
                preambleHeaderBlock.Write(testStream);
                testStream.Position = 0;
                using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream))
                {
                    Assert.That(axCryptReader.Read(), Is.True, "We should be able to read the Guid");
                    Assert.That(axCryptReader.ItemType, Is.EqualTo(AxCryptItemType.MagicGuid), "We're expecting to have found a MagicGuid");
                    Assert.That(axCryptReader.Read(), Is.True, "We should be able to read the next HeaderBlock");
                    Assert.That(axCryptReader.ItemType, Is.EqualTo(AxCryptItemType.HeaderBlock), "We're expecting to have found a HeaderBlock");
                    Assert.That(axCryptReader.HeaderBlock.HeaderBlockType, Is.EqualTo(HeaderBlockType.Preamble), "We're expecting to have found a Preamble specifically");
                    Assert.Throws<FileFormatException>(() => axCryptReader.Read());
                }
            }
        }

        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This is test, readability and coding ease is a concern, not performance.")]
        public void TestFindPreambleHeaderBlockNotFirstShouldThrow()
        {
            using (MemoryStream testStream = new MemoryStream())
            {
                AxCrypt1Guid.Write(testStream);
                VersionHeaderBlock versionHeaderBlock = new VersionHeaderBlock();
                versionHeaderBlock.Write(testStream);
                PreambleHeaderBlock preambleHeaderBlock = new PreambleHeaderBlock();
                preambleHeaderBlock.Write(testStream);
                testStream.Position = 0;
                using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream))
                {
                    Assert.That(axCryptReader.Read(), Is.True, "We should be able to read the Guid");
                    Assert.That(axCryptReader.ItemType, Is.EqualTo(AxCryptItemType.MagicGuid), "We're expecting to have found a MagicGuid");
                    Assert.Throws<FileFormatException>(() => axCryptReader.Read());
                }
            }
        }

        [Test]
        public static void TestFindPreambleHeaderBlockFromSimpleFile()
        {
            using (Stream testStream = new MemoryStream(Resources.HelloWorld_Key_a_txt))
            {
                using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream))
                {
                    bool blockFound = false;
                    int headers = 0;
                    while (axCryptReader.Read())
                    {
                        switch (axCryptReader.ItemType)
                        {
                            case AxCryptItemType.None:
                                break;
                            case AxCryptItemType.MagicGuid:
                                break;
                            case AxCryptItemType.HeaderBlock:
                                if (axCryptReader.HeaderBlock.HeaderBlockType == HeaderBlockType.Preamble)
                                {
                                    Assert.That(blockFound, Is.False, "We should only find one single PreambleHeaderBlock");
                                    blockFound = true;

                                    Assert.That(headers, Is.EqualTo(0), "Preamble must be first");
                                    PreambleHeaderBlock preambleHeaderBlock = (PreambleHeaderBlock)axCryptReader.HeaderBlock;
                                    Assert.That(preambleHeaderBlock.GetHmac().Length, Is.EqualTo(16), "The HMAC in the preamble must be exactly 16 bytes.");
                                }
                                ++headers;
                                break;
                            case AxCryptItemType.Data:
                                break;
                            case AxCryptItemType.EndOfStream:
                                break;
                            default:
                                break;
                        }
                    }
                    Assert.That(blockFound, Is.True, "We're expecting a VersionHeaderBlock to be found!");
                }
            }
        }
    }
}
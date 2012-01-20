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
                using (AxCryptReader axCryptReader = new AxCryptReader(testStream))
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
                using (AxCryptReader axCryptReader = new AxCryptReader(testStream))
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
                using (AxCryptReader axCryptReader = new AxCryptReader(testStream))
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
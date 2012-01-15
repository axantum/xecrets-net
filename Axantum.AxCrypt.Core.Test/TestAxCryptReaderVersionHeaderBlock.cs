using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using Axantum.AxCrypt.Core.Test.Properties;
using Axantum.AxCrypt.Core.Reader;
using Axantum.AxCrypt.Core.Header;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestAxCryptReaderVersionHeaderBlock
    {
        [Test]
        public static void TestFindVersionHeaderBlockFromSimpleFile()
        {
            using (Stream testStream = new MemoryStream(Resources.HelloWorld_Key_a_txt))
            {
                using (AxCryptReader axCryptReader = new AxCryptReader(testStream))
                {
                    bool blockFound = false;
                    while (axCryptReader.Read())
                    {
                        switch (axCryptReader.ItemType)
                        {
                            case AxCryptItemType.None:
                                break;
                            case AxCryptItemType.MagicGuid:
                                break;
                            case AxCryptItemType.HeaderBlock:
                                if (axCryptReader.HeaderBlock is VersionHeaderBlock)
                                {
                                    Assert.That(blockFound, Is.False, "We should only find one single VersionHeaderBlock");
                                    blockFound = true;
                                }
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

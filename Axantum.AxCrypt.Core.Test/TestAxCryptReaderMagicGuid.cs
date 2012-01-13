using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.Reader;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public class TestAxCryptReaderMagicGuid
    {
        private byte[] _axCrypt1GuidAsBytes;

        [SetUp]
        public void Setup()
        {
            _axCrypt1GuidAsBytes = AxCryptReader.GetAxCrypt1GuidAsBytes();
        }

        [Test]
        public void TestFindMagicGuidFirstAndOnly()
        {
            using (MemoryStream testStream = new MemoryStream())
            {
                testStream.Write(_axCrypt1GuidAsBytes, 0, _axCrypt1GuidAsBytes.Length);
                testStream.Position = 0;
                using (AxCryptReader axCryptReader = new AxCryptReader(testStream))
                {
                    Assert.That(axCryptReader.Read(), Is.True, "We should be able to read the Guid");
                    Assert.That(axCryptReader.ItemType, Is.EqualTo(AxCryptItemType.MagicGuid), "We're expecting to have found a MagicGuid");
                }
            }
        }

        [Test]
        public void TestFindMagicGuidFirstWithMore()
        {
            using (MemoryStream testStream = new MemoryStream())
            {
                testStream.Write(_axCrypt1GuidAsBytes, 0, _axCrypt1GuidAsBytes.Length);
                byte[] someBytes = Encoding.UTF8.GetBytes("This is a test string that we'll convert into some random bytes....");
                testStream.Write(someBytes, 0, someBytes.Length);
                testStream.Position = 0;
                using (AxCryptReader axCryptReader = new AxCryptReader(testStream))
                {
                    Assert.That(axCryptReader.Read(), Is.True, "We should be able to read the Guid");
                    Assert.That(axCryptReader.ItemType, Is.EqualTo(AxCryptItemType.MagicGuid), "We're expecting to have found a MagicGuid");
                }
            }
        }

        [Test]
        public void TestFindMagicGuidWithOtherFirstButNoMore()
        {
            using (MemoryStream testStream = new MemoryStream())
            {
                byte[] someBytes = Encoding.UTF8.GetBytes("This is a test string that we'll convert into some random bytes....");
                testStream.Write(someBytes, 0, someBytes.Length);
                testStream.Write(_axCrypt1GuidAsBytes, 0, _axCrypt1GuidAsBytes.Length);
                testStream.Position = 0;
                using (AxCryptReader axCryptReader = new AxCryptReader(testStream))
                {
                    Assert.That(axCryptReader.Read(), Is.True, "We should be able to read the Guid");
                    Assert.That(axCryptReader.ItemType, Is.EqualTo(AxCryptItemType.MagicGuid), "We're expecting to have found a MagicGuid");
                }
            }
        }

        [Test]
        public void TestFindMagicGuidWithOtherFirstAndMore()
        {
            using (MemoryStream testStream = new MemoryStream())
            {
                byte[] someBytes = Encoding.UTF8.GetBytes("This is a test string that we'll convert into some random bytes....");
                testStream.Write(someBytes, 0, someBytes.Length);
                testStream.Write(_axCrypt1GuidAsBytes, 0, _axCrypt1GuidAsBytes.Length);
                testStream.Write(someBytes, 0, someBytes.Length);
                testStream.Position = 0;
                using (AxCryptReader axCryptReader = new AxCryptReader(testStream))
                {
                    Assert.That(axCryptReader.Read(), Is.True, "We should be able to read the Guid");
                    Assert.That(axCryptReader.ItemType, Is.EqualTo(AxCryptItemType.MagicGuid), "We're expecting to have found a MagicGuid");
                }
            }
        }
    }
}
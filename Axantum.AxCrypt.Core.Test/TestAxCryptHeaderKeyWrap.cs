using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Header;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public class TestAxCryptHeaderKeyWrap
    {
        private static byte[] _keyEncryptingKey = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };
        private static byte[] _keyData = new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF };
        private static byte[] _a = new byte[] { 0xA6, 0xA6, 0xA6, 0xA6, 0xA6, 0xA6, 0xA6, 0xA6 };
        private static byte[] _wrapped = new byte[] { 0x1F, 0xA6, 0x8B, 0x0A, 0x81, 0x12, 0xB4, 0x47, 0xAE, 0xF3, 0x4B, 0xD8, 0xFB, 0x5A, 0x7B, 0x82, 0x9D, 0x3E, 0x86, 0x23, 0x71, 0xD2, 0xCF, 0xE5 };
        private static byte[] _unwrapped = _a.Append(_keyData);

        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This is test, readability and coding ease is a concern, not performance.")]
        public void TestUnwrap()
        {
            KeyWrap keyWrap = new KeyWrap(_keyEncryptingKey, new byte[] { }, 6);
            byte[] unwrapped = keyWrap.Unwrap(_wrapped);

            Assert.That(unwrapped, Is.EquivalentTo(_unwrapped), "Unwrapped the wrong data");
        }
    }
}
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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Header;
using Axantum.AxCrypt.Core.Reader;
using Axantum.AxCrypt.Core.Test.Properties;
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
            byte[] unwrapped;
            using (KeyWrap keyWrap = new KeyWrap(_keyEncryptingKey, new byte[] { }, 6, KeyWrapMode.Specification))
            {
                unwrapped = keyWrap.Unwrap(_wrapped);
            }

            Assert.That(unwrapped, Is.EquivalentTo(_unwrapped), "Unwrapped the wrong data");
        }

        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This is test, readability and coding ease is a concern, not performance.")]
        public void TestUnwrapWithBadArgument()
        {
            using (KeyWrap keyWrap = new KeyWrap(_keyEncryptingKey, new byte[] { }, 6, KeyWrapMode.Specification))
            {
                Assert.Throws<ArgumentException>(() => { keyWrap.Unwrap(_keyData); }, "Calling with too short wrapped data");
            }

            using (KeyWrap keyWrap = new KeyWrap(_keyEncryptingKey, new byte[] { }, 6, (KeyWrapMode)9999))
            {
                Assert.Throws<InvalidOperationException>(() => { keyWrap.Unwrap(_wrapped); }, "Calling with a bogus KeyWrapMode.");
            }
        }

        [Test]
        public static void TestUnwrapFromSimpleFile()
        {
            using (Stream testStream = new MemoryStream(Resources.HelloWorld_Key_a_txt))
            {
                KeyWrap1HeaderBlock keyWrapHeaderBlock = null;
                using (AxCryptReader axCryptReader = AxCryptReader.Create(testStream))
                {
                    int headers = 0;
                    while (axCryptReader.Read())
                    {
                        switch (axCryptReader.CurrentItemType)
                        {
                            case AxCryptItemType.None:
                                break;
                            case AxCryptItemType.MagicGuid:
                                break;
                            case AxCryptItemType.HeaderBlock:
                                if (axCryptReader.CurrentHeaderBlock.HeaderBlockType == HeaderBlockType.KeyWrap1)
                                {
                                    keyWrapHeaderBlock = (KeyWrap1HeaderBlock)axCryptReader.CurrentHeaderBlock;
                                    ++headers;
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
                    Assert.That(headers, Is.EqualTo(1), "We're expecting exactly one KeyWrap1 block to be found!");
                    byte[] salt = keyWrapHeaderBlock.GetSalt();
                    byte[] wrapped = keyWrapHeaderBlock.GetKeyData();
                    long iterations = keyWrapHeaderBlock.Iterations();
                    AxCryptReaderSettings readerSettings = new AxCryptReaderSettings("a");
                    using (KeyWrap keyWrap = new KeyWrap(readerSettings.GetDerivedPassphrase(), salt, iterations, KeyWrapMode.AxCrypt))
                    {
                        byte[] unwrapped = keyWrap.Unwrap(wrapped);
                        byte[] a = new byte[8];
                        Array.Copy(unwrapped, 0, a, 0, 8);
                        Assert.That(a, Is.EquivalentTo(_a), "An unwrapped key should always start with the known IV 'A' from the specification.");
                    }
                }
            }
        }

        private class EnvironmentForTest : IEnvironment
        {
            public bool IsLittleEndian
            {
                get { return !BitConverter.IsLittleEndian; }
            }
        }

        private class KeyWrapForTest : KeyWrap
        {
            public KeyWrapForTest(byte[] key, byte[] salt, long iterations, KeyWrapMode mode)
                : base(key, salt, iterations, mode, new EnvironmentForTest())
            {
            }

            public new byte[] GetBigEndianBytes(long value)
            {
                return base.GetBigEndianBytes(value);
            }

            public new byte[] GetLittleEndianBytes(long value)
            {
                return base.GetLittleEndianBytes(value);
            }
        }

        [Test]
        public static void TestEndianOptimization()
        {
            long iterations = 1000;
            byte[] salt = new byte[16];
            AxCryptReaderSettings readerSettings = new AxCryptReaderSettings("a");
            using (KeyWrapForTest keyWrap = new KeyWrapForTest(readerSettings.GetDerivedPassphrase(), salt, iterations, KeyWrapMode.AxCrypt))
            {
                if (BitConverter.IsLittleEndian)
                {
                    byte[] actuallyLittleEndianBytes = keyWrap.GetBigEndianBytes(0x0102030405060708);
                    Assert.That(actuallyLittleEndianBytes, Is.EqualTo(new byte[] { 0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01 }), "Getting big endian, thinking we are big endian but in fact are not, will get us little endian bytes.");

                    byte[] actuallyStillLittleEndianBytes = keyWrap.GetLittleEndianBytes(0x0102030405060708);
                    Assert.That(actuallyStillLittleEndianBytes, Is.EqualTo(new byte[] { 0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01 }), "Getting little endian, thinking we are big endian but in fact are not, will still get us little endian.");
                }
                else
                {
                    byte[] actuallyStillBigEndianBytes = keyWrap.GetBigEndianBytes(0x0102030405060708);
                    Assert.That(actuallyStillBigEndianBytes, Is.EqualTo(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 }), "Getting big endian, thinking we are little endian but in fact are not, will still get us big endian bytes.");

                    byte[] actuallyBigEndianBytes = keyWrap.GetLittleEndianBytes(0x0102030405060708);
                    Assert.That(actuallyBigEndianBytes, Is.EqualTo(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 }), "Getting little endian, thinking we are big endian but in fact are not, will get us big endian bytes.");
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Reader;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public class TestKeyWrap
    {
        private static AesKey _keyEncryptingKey = new AesKey(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F });
        private static AesKey _keyData = new AesKey(new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF });
        private static byte[] _wrapped = new byte[] { 0x1F, 0xA6, 0x8B, 0x0A, 0x81, 0x12, 0xB4, 0x47, 0xAE, 0xF3, 0x4B, 0xD8, 0xFB, 0x5A, 0x7B, 0x82, 0x9D, 0x3E, 0x86, 0x23, 0x71, 0xD2, 0xCF, 0xE5 };

        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This is test, readability and coding ease is a concern, not performance.")]
        public void TestUnwrap()
        {
            byte[] unwrapped;
            using (KeyWrap keyWrap = new KeyWrap(_keyEncryptingKey, 6, KeyWrapMode.Specification))
            {
                unwrapped = keyWrap.Unwrap(_wrapped);
            }

            Assert.That(unwrapped, Is.EquivalentTo(_keyData.GetBytes()), "Unwrapped the wrong data");
        }

        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This is test, readability and coding ease is a concern, not performance.")]
        public void TestWrap()
        {
            byte[] wrapped;
            using (KeyWrap keyWrap = new KeyWrap(_keyEncryptingKey, 6, KeyWrapMode.Specification))
            {
                wrapped = keyWrap.Wrap(_keyData);
            }

            Assert.That(wrapped, Is.EquivalentTo(_wrapped), "The wrapped data is not correct according to specfication.");
        }

        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This is test, readability and coding ease is a concern, not performance.")]
        public void TestWrapAndUnwrapAxCryptMode()
        {
            AesKey keyToWrap = new AesKey(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });
            AesKey keyEncryptingKey = new AesKey(new byte[] { 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 });
            byte[] salt = new byte[] { 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 };
            long iterations = 12345;
            byte[] wrapped;
            using (KeyWrap keyWrap = new KeyWrap(keyEncryptingKey, salt, iterations, KeyWrapMode.AxCrypt))
            {
                wrapped = keyWrap.Wrap(keyToWrap);
            }
            byte[] unwrapped;
            using (KeyWrap keyWrap = new KeyWrap(keyEncryptingKey, salt, iterations, KeyWrapMode.AxCrypt))
            {
                unwrapped = keyWrap.Unwrap(wrapped);
            }

            Assert.That(unwrapped, Is.EquivalentTo(keyToWrap.GetBytes()), "The unwrapped data should be equal to original.");
        }

        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This is test, readability and coding ease is a concern, not performance.")]
        public void TestWrapAndUnwrapSpecificationMode()
        {
            AesKey keyToWrap = new AesKey(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });
            AesKey keyEncryptingKey = new AesKey(new byte[] { 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 });
            byte[] salt = new byte[] { 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 };
            long iterations = 23456;
            byte[] wrapped;
            using (KeyWrap keyWrap = new KeyWrap(keyEncryptingKey, salt, iterations, KeyWrapMode.Specification))
            {
                wrapped = keyWrap.Wrap(keyToWrap);
            }
            byte[] unwrapped;
            using (KeyWrap keyWrap = new KeyWrap(keyEncryptingKey, salt, iterations, KeyWrapMode.Specification))
            {
                unwrapped = keyWrap.Unwrap(wrapped);
            }

            Assert.That(unwrapped, Is.EquivalentTo(keyToWrap.GetBytes()), "The unwrapped data should be equal to original.");
        }

        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This is test, readability and coding ease is a concern, not performance.")]
        public void TestKeyWrapConstructorWithBadArgument()
        {
            using (KeyWrap keyWrap = new KeyWrap(_keyEncryptingKey, 6, KeyWrapMode.Specification))
            {
                Assert.Throws<ArgumentException>(() => { keyWrap.Unwrap(_keyData.GetBytes()); }, "Calling with too short wrapped data.");
            }

            Assert.Throws<ArgumentException>(() =>
            {
                using (KeyWrap keyWrap = new KeyWrap(_keyEncryptingKey, new byte[] { 0, 1, 3 }, 6, KeyWrapMode.AxCrypt))
                {
                }
            }, "Calling with too short salt.");

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                using (KeyWrap keyWrap = new KeyWrap(_keyEncryptingKey, 5, KeyWrapMode.AxCrypt))
                {
                }
            }, "Calling with too few iterations.");

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                using (KeyWrap keyWrap = new KeyWrap(_keyEncryptingKey, 0, KeyWrapMode.AxCrypt))
                {
                }
            }, "Calling with zero (too few) iterations.");

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                using (KeyWrap keyWrap = new KeyWrap(_keyEncryptingKey, -100, KeyWrapMode.AxCrypt))
                {
                }
            }, "Calling with negative number of iterations.");

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                using (KeyWrap keyWrap = new KeyWrap(_keyEncryptingKey, 6, (KeyWrapMode)9999))
                {
                }
            }, "Calling with bogus KeyWrapMode.");
        }
    }
}
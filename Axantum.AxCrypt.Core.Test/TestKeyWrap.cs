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
using System.Diagnostics.CodeAnalysis;
using Axantum.AxCrypt.Core.Crypto;
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

            Assert.That(wrapped, Is.EquivalentTo(_wrapped), "The wrapped data is not correct according to specification.");

            using (KeyWrap keyWrap = new KeyWrap(_keyEncryptingKey, 6, KeyWrapMode.Specification))
            {
                Assert.Throws<ArgumentNullException>(() =>
                {
                    wrapped = keyWrap.Wrap(null);
                });
            }
        }

        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This is test, readability and coding ease is a concern, not performance.")]
        public void TestWrapAndUnwrapAxCryptMode()
        {
            AesKey keyToWrap = new AesKey(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });
            AesKey keyEncryptingKey = new AesKey(new byte[] { 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 });
            KeyWrapSalt salt = new KeyWrapSalt(new byte[] { 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 });
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
            KeyWrapSalt salt = new KeyWrapSalt(new byte[] { 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 });
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
                Assert.Throws<InternalErrorException>(() => { keyWrap.Unwrap(_keyData.GetBytes()); }, "Calling with too short wrapped data.");
            }

            Assert.Throws<InternalErrorException>(() =>
            {
                using (KeyWrap keyWrap = new KeyWrap(_keyEncryptingKey, new KeyWrapSalt(32), 6, KeyWrapMode.AxCrypt))
                {
                }
            }, "Calling with too long salt.");

            Assert.Throws<InternalErrorException>(() =>
            {
                using (KeyWrap keyWrap = new KeyWrap(_keyEncryptingKey, 5, KeyWrapMode.AxCrypt))
                {
                }
            }, "Calling with too few iterations.");

            Assert.Throws<InternalErrorException>(() =>
            {
                using (KeyWrap keyWrap = new KeyWrap(_keyEncryptingKey, 0, KeyWrapMode.AxCrypt))
                {
                }
            }, "Calling with zero (too few) iterations.");

            Assert.Throws<InternalErrorException>(() =>
            {
                using (KeyWrap keyWrap = new KeyWrap(_keyEncryptingKey, -100, KeyWrapMode.AxCrypt))
                {
                }
            }, "Calling with negative number of iterations.");

            Assert.Throws<InternalErrorException>(() =>
            {
                using (KeyWrap keyWrap = new KeyWrap(_keyEncryptingKey, 6, (KeyWrapMode)9999))
                {
                }
            }, "Calling with bogus KeyWrapMode.");

            Assert.Throws<ArgumentNullException>(() =>
            {
                using (KeyWrap keyWrap = new KeyWrap(null, new KeyWrapSalt(16), 6, KeyWrapMode.AxCrypt))
                {
                }
            }, "Calling with null key argument.");

            Assert.Throws<ArgumentNullException>(() =>
            {
                using (KeyWrap keyWrap = new KeyWrap(_keyEncryptingKey, null, 6, KeyWrapMode.Specification))
                {
                }
            }, "Calling with null salt argument.");
        }

        [Test]
        public static void TestDispose()
        {
            KeyWrap keyWrap = new KeyWrap(_keyEncryptingKey, 6, KeyWrapMode.Specification);
            keyWrap.Dispose();
            Assert.Throws<ObjectDisposedException>(() =>
            {
                keyWrap.Wrap(new AesKey());
            });

            Assert.Throws<ObjectDisposedException>(() =>
            {
                keyWrap.Unwrap(new byte[16 + 8]);
            });

            Assert.DoesNotThrow(() =>
            {
                keyWrap.Dispose();
            });
        }
    }
}
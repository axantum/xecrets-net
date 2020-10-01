#if DEBUG
#define CODE_ANALYSIS
#endif

#region License

/*
 *  AxCrypt.Xecrets.Core - Xecrets Core and Reference Implementation
 *
 *  Copyright (C) 2008 Svante Seleborg
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  If you'd like to license this program under any other terms than the
 *  above, please contact the author and copyright holder.
 *
 *  Contact: mailto:svante@axantum.com
 */

#endregion License

using System;
using System.Diagnostics;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Secrets
{
    /// <summary>
    /// An immutable encryption key container.
    /// </summary>
    /// <remarks>
    /// Instances of this class cannot be serialized. Do not place in a location that survives an AppDomain restart.
    /// </remarks>
    public class EncryptionKey : IEquatable<EncryptionKey>
    {
        private byte[] _bytes;

        /// <summary>
        /// Encrypts a string for use in this AppDomain. The string can only be decrypted in the
        /// lifetime of this AppDomain. The decryption method is not publically visible.
        /// </summary>
        /// <param name="keyString">The password etc. It is assumed to be normalized by the caller.</param>
        public EncryptionKey(string passphrase)
        {
            if (passphrase == null)
            {
                throw new ArgumentNullException("passphrase");
            }

            _bytes = New<TransientProtectedData>().Protect(passphrase);
        }

        public EncryptionKey(EncryptionKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            _bytes = key._bytes;
        }

        /// <summary>
        /// Decrypts a runtime-encrypted string. Can only decrypt a string that was encrypted
        /// in the lifetime of this AppDomain.
        /// </summary>
        /// <returns>The original string</returns>
        protected internal string DecryptPassphrase()
        {
            string keyString;
            if (!New<TransientProtectedData>().TryUnprotect(_bytes, out keyString))
            {
                return null;
            }
            return keyString;
        }

        /// <summary>
        /// Compares two keys. Note that how to do this is private knowledge that cannot be exposed outside
        /// the provider, which is why we use the indirect method of having this code here instead of in
        /// the EncryptionKey class.
        /// </summary>
        /// <param name="key1">The key1.</param>
        /// <param name="key2">The key2.</param>
        /// <returns></returns>
        private static bool KeyComparer(EncryptionKey key1, EncryptionKey key2)
        {
            if (key1._bytes.Length != key2._bytes.Length)
            {
                return false;
            }

            return String.Compare(key1.DecryptPassphrase(), key2.DecryptPassphrase(), StringComparison.Ordinal) == 0;
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="key1">The key1.</param>
        /// <param name="key2">The key2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(EncryptionKey key1, EncryptionKey key2)
        {
            if (System.Object.ReferenceEquals(key1, key2))
            {
                return true;
            }

            if (((object)key1) == null || ((object)key2 == null))
            {
                return false;
            }

            return KeyComparer(key1, key2);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="key1">The key1.</param>
        /// <param name="key2">The key2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(EncryptionKey key1, EncryptionKey key2)
        {
            return !(key1 == key2);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as EncryptionKey);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #region IEquatable<EncryptionKey> Members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the other parameter; otherwise, false.
        /// </returns>
        public bool Equals(EncryptionKey other)
        {
            return this == other;
        }

        #endregion IEquatable<EncryptionKey> Members
    }
}
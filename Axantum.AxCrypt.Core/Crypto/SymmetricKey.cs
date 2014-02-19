#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
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

using Axantum.AxCrypt.Core.Extensions;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Axantum.AxCrypt.Core.Crypto
{
    /// <summary>
    /// Hold a key for a symmetric algorithm. Instances of this class are immutable.
    /// </summary>
    public class SymmetricKey : IEquatable<SymmetricKey>
    {
        private byte[] _symmetricKey;

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "This type is immutable.")]
        public static readonly SymmetricKey Zero128 = new SymmetricKey(new byte[16], SymmetricKeyThumbprint.Zero);

        private SymmetricKey(byte[] key, SymmetricKeyThumbprint thumbprint)
            : this(key)
        {
            Thumbprint = thumbprint;
        }

        /// <summary>
        /// Instantiate a random key.
        /// </summary>
        public SymmetricKey(int keyBits)
            : this(Instance.RandomGenerator.Generate(keyBits / 8))
        {
        }

        /// <summary>
        /// Instantiate a key.
        /// </summary>
        /// <param name="key">The key to use. The length can be any that is valid for the algorithm.</param>
        public SymmetricKey(byte[] key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            _symmetricKey = (byte[])key.Clone();
        }

        /// <summary>
        /// Get the actual key bytes.
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            return (byte[])_symmetricKey.Clone();
        }

        public int Length
        {
            get
            {
                return _symmetricKey.Length;
            }
        }

        private SymmetricKeyThumbprint _thumbprint;

        public SymmetricKeyThumbprint Thumbprint
        {
            get
            {
                if (_thumbprint == null)
                {
                    _thumbprint = new SymmetricKeyThumbprint(this, Instance.UserSettings.ThumbprintSalt, Instance.UserSettings.V1KeyWrapIterations);
                }
                return _thumbprint;
            }
            set
            {
                _thumbprint = value;
            }
        }

        #region IEquatable<SymmetricKey> Members

        /// <summary>
        /// Check if one instance is equivalent to another.
        /// </summary>
        /// <param name="other">The instance to compare to</param>
        /// <returns>true if the keys are equivalent</returns>
        public bool Equals(SymmetricKey other)
        {
            if ((object)other == null)
            {
                return false;
            }
            return _symmetricKey.IsEquivalentTo(other._symmetricKey);
        }

        #endregion IEquatable<SymmetricKey> Members

        public override bool Equals(object obj)
        {
            if (obj == null || typeof(SymmetricKey) != obj.GetType())
            {
                return false;
            }
            SymmetricKey other = (SymmetricKey)obj;

            return Equals(other);
        }

        public override int GetHashCode()
        {
            int hashcode = 0;
            foreach (byte b in _symmetricKey)
            {
                hashcode += b;
            }
            return hashcode;
        }

        public static bool operator ==(SymmetricKey left, SymmetricKey right)
        {
            if (Object.ReferenceEquals(left, right))
            {
                return true;
            }
            if ((object)left == null)
            {
                return false;
            }
            return left.Equals(right);
        }

        public static bool operator !=(SymmetricKey left, SymmetricKey right)
        {
            return !(left == right);
        }
    }
}
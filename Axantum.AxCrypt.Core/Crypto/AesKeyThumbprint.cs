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
using System.Runtime.Serialization;
using System.Security.Cryptography;

namespace Axantum.AxCrypt.Core.Crypto
{
    /// <summary>
    /// Represent a salted thumbprint for a AES key. Instances of this class are immutable.
    /// </summary>
    [DataContract(Namespace = "http://www.axantum.com/Serialization/")]
    public class AesKeyThumbprint : IEquatable<AesKeyThumbprint>
    {
        [DataMember(Name = "Thumbprint")]
        private byte[] _bytes;

        public AesKeyThumbprint(AesKey key)
            : this(key, OS.Current.GetRandomBytes(32))
        {
        }

        /// <summary>
        /// Instantiate a thumbprint
        /// </summary>
        /// <param name="key">The key to thumbprint.</param>
        /// <param name="salt">The salt to use.</param>
        public AesKeyThumbprint(AesKey key, byte[] salt)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (salt == null)
            {
                throw new ArgumentNullException("salt");
            }

            HashAlgorithm hash = HashAlgorithm.Create("SHA256");
            hash.TransformBlock(salt, 0, salt.Length, null, 0);
            hash.TransformFinalBlock(key.GetBytes(), 0, key.Length);

            _bytes = new byte[sizeof(int) + salt.Length + hash.Hash.Length];
            BitConverter.GetBytes(salt.Length).CopyTo(_bytes, 0);
            salt.CopyTo(_bytes, sizeof(int));
            hash.Hash.CopyTo(_bytes, sizeof(int) + salt.Length);
        }

        public byte[] GetSalt()
        {
            int saltLength = BitConverter.ToInt32(_bytes, 0);
            byte[] salt = new byte[saltLength];
            Array.Copy(_bytes, sizeof(int), salt, 0, salt.Length);
            return salt;
        }

        /// <summary>
        /// Get the thumbprint.
        /// </summary>
        /// <returns>Calculate and return the thumbprint.</returns>
        public byte[] GetBytes()
        {
            return (byte[])_bytes.Clone();
        }

        /// <summary>
        /// Compare this instance to a set of other thumbprints for equivalence.
        /// </summary>
        /// <param name="thumbprints">A collection of thumbprints to match with this one.</param>
        /// <returns>True if any of the thumbprints in the colleciton matches this one, otherwise False</returns>
        public bool MatchAny(IEnumerable<AesKeyThumbprint> thumbprints)
        {
            foreach (AesKeyThumbprint thumbprint in thumbprints)
            {
                if (this == thumbprint)
                {
                    return true;
                }
            }
            return false;
        }

        #region IEquatable<AesKeyThumbprint> Members

        public bool Equals(AesKeyThumbprint other)
        {
            if ((object)other == null)
            {
                return false;
            }
            return _bytes.IsEquivalentTo(other._bytes);
        }

        #endregion IEquatable<AesKeyThumbprint> Members

        public override bool Equals(object obj)
        {
            if (obj == null || typeof(AesKeyThumbprint) != obj.GetType())
            {
                return false;
            }
            AesKeyThumbprint other = (AesKeyThumbprint)obj;

            return Equals(other);
        }

        public override int GetHashCode()
        {
            int hashcode = 0;
            foreach (byte b in _bytes)
            {
                hashcode += b;
            }
            return hashcode;
        }

        public static bool operator ==(AesKeyThumbprint left, AesKeyThumbprint right)
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

        public static bool operator !=(AesKeyThumbprint left, AesKeyThumbprint right)
        {
            return !(left == right);
        }
    }
}
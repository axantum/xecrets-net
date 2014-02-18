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
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Axantum.AxCrypt.Core.Crypto
{
    /// <summary>Implements password-based key derivation functionality, PBKDF2, by using a pseudo-random number generator based on <see cref="T:System.Security.Cryptography.HMACSHA512" />.</summary>
    [ComVisible(true)]
    public class Pbkdf2HmacSha512 : IDisposable
    {
        private byte[] m_buffer;
        private byte[] m_salt;
        private HMACSHA512 m_hmacsha1;
        private int m_iterations;
        private int m_block;
        private int m_startIndex;
        private int m_endIndex;

        /// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Rfc2898DeriveBytes" /> class using a password, a salt, and number of iterations to derive the key.</summary>
        /// <param name="password">The password used to derive the key. </param>
        /// <param name="salt">The key salt used to derive the key.</param>
        /// <param name="iterations">The number of iterations for the operation. </param>
        /// <exception cref="T:System.ArgumentException">The specified salt size is smaller than 8 bytes or the iteration count is less than 1. </exception>
        /// <exception cref="T:System.ArgumentNullException">The password or salt is null. </exception>
        public Pbkdf2HmacSha512(string password, byte[] salt, int iterations)
        {
            m_salt = salt;
            m_iterations = iterations;
            m_hmacsha1 = new HMACSHA512(new UTF8Encoding(false).GetBytes(password));
            Initialize();
        }

        /// <summary>Returns the pseudo-random key for this object.</summary>
        /// <returns>A byte array filled with pseudo-random key bytes.</returns>
        public byte[] GetBytes()
        {
            byte[] array = new byte[64];
            int i = 0;
            int num = m_endIndex - m_startIndex;
            if (num > 0)
            {
                if (array.Length < num)
                {
                    Array.Copy(m_buffer, m_startIndex, array, 0, array.Length);
                    m_startIndex += array.Length;
                    return array;
                }
                Array.Copy(m_buffer, m_startIndex, array, 0, num);
                m_startIndex = m_endIndex = 0;
                i += num;
            }
            while (i < array.Length)
            {
                byte[] src = Func();
                int num2 = array.Length - i;
                int blockLength = m_hmacsha1.HashSize / 8;
                if (num2 <= blockLength)
                {
                    Array.Copy(src, 0, array, i, num2);
                    i += num2;
                    Array.Copy(src, num2, m_buffer, m_startIndex, blockLength - num2);
                    m_endIndex += blockLength - num2;
                    return array;
                }
                Array.Copy(src, 0, array, i, blockLength);
                i += blockLength;
            }
            return array;
        }

        /// <summary>Releases the unmanaged resources used by the <see cref="T:System.Security.Cryptography.Rfc2898DeriveBytes" /> class and optionally releases the managed resources.</summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeInternal();
            }
        }

        private void DisposeInternal()
        {
            if (m_hmacsha1 != null)
            {
                ((IDisposable)m_hmacsha1).Dispose();
                m_hmacsha1 = null;
            }
            if (m_buffer != null)
            {
                Array.Clear(m_buffer, 0, m_buffer.Length);
                m_buffer = null;
            }
            if (m_salt != null)
            {
                Array.Clear(m_salt, 0, m_salt.Length);
                m_salt = null;
            }
        }

        private void Initialize()
        {
            if (m_buffer != null)
            {
                Array.Clear(m_buffer, 0, m_buffer.Length);
            }
            m_buffer = new byte[64];
            m_block = 1;
            m_startIndex = m_endIndex = 0;
        }

        private byte[] Func()
        {
            byte[] array = m_block.GetBigEndianBytes();

            m_hmacsha1.TransformBlock(m_salt, 0, m_salt.Length, null, 0);
            m_hmacsha1.TransformBlock(array, 0, array.Length, null, 0);
            m_hmacsha1.TransformFinalBlock(new byte[0], 0, 0);
            byte[] hashValue = m_hmacsha1.Hash;

            m_hmacsha1.Initialize();
            byte[] array2 = hashValue;
            int num = 2;
            while (num <= m_iterations)
            {
                m_hmacsha1.TransformBlock(hashValue, 0, hashValue.Length, null, 0);
                m_hmacsha1.TransformFinalBlock(new byte[0], 0, 0);
                hashValue = m_hmacsha1.Hash;
                for (int i = 0; i < 64; i++)
                {
                    byte[] expr_AB_cp_0 = array2;
                    int expr_AB_cp_1 = i;
                    expr_AB_cp_0[expr_AB_cp_1] ^= hashValue[i];
                }
                m_hmacsha1.Initialize();
                num++;
            }
            m_block += 1;
            return array2;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
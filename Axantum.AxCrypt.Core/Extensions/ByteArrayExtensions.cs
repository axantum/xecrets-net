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

using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Extensions
{
    public static class ByteArrayExtensions
    {
        /// <summary>
        /// Naive implementation of IndexOf - optimize only if it proves necessary. Look for Boyer Moore.
        /// </summary>
        /// <param name="buffer">The buffer to search in</param>
        /// <param name="pattern">The pattern to search for</param>
        /// <param name="offset">Where to start the search in buffer</param>
        /// <param name="count">How many bytes to include in the search</param>
        /// <returns>The location in the buffer of the pattern, or -1 if not found</returns>
        public static int Locate(this byte[] buffer, byte[] pattern, int offset, int count)
        {
            return buffer.Locate(pattern, offset, count, 1);
        }

        /// <summary>
        /// Naive implementation of IndexOf - optimize only if it proves necessary. Look for Boyer Moore.
        /// </summary>
        /// <param name="buffer">The buffer to search in</param>
        /// <param name="pattern">The pattern to search for</param>
        /// <param name="offset">Where to start the search in buffer</param>
        /// <param name="count">How many bytes to include in the search</param>
        /// <param name="increment">How much to increment when stepping forward</param>
        /// <returns>The location in the buffer of the pattern, or -1 if not found</returns>
        public static int Locate(this byte[] buffer, byte[] pattern, int offset, int count, int increment)
        {
            int candidatePosition = offset;
            while (candidatePosition - offset + pattern.Length <= count)
            {
                int i;
                for (i = 0; i < pattern.Length; i += increment)
                {
                    int j;
                    for (j = 0; j < increment; ++j)
                    {
                        if (buffer[candidatePosition + i + j] != pattern[i + j])
                        {
                            break;
                        }
                    }
                    if (j < increment)
                    {
                        break;
                    }
                }
                if (i == pattern.Length)
                {
                    return candidatePosition;
                }
                candidatePosition += increment;
            }
            return -1;
        }

        public static byte[] Xor(this byte[] buffer, byte[] other)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            int bytesToXor = buffer.Length < other.Length ? buffer.Length : other.Length;
            buffer.Xor(0, other, 0, bytesToXor);
            return buffer;
        }

        public static byte[] Xor(this byte[] buffer, int bufferIndex, byte[] other, int otherIndex, int length)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            if (bufferIndex + length > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("length");
            }
            if (otherIndex + length > other.Length)
            {
                throw new ArgumentOutOfRangeException("length");
            }
            for (int i = 0; i < length; ++i)
            {
                buffer[bufferIndex + i] ^= other[otherIndex + i];
            }
            return buffer;
        }

        public static byte[] Append(this byte[] left, params byte[][] arrays)
        {
            int length = 0;
            foreach (byte[] array in arrays)
            {
                length += array.Length;
            }
            length += left.Length;
            byte[] concatenatedArray = new byte[length];
            left.CopyTo(concatenatedArray, 0);
            int index = left.Length;
            foreach (byte[] array in arrays)
            {
                array.CopyTo(concatenatedArray, index);
                index += array.Length;
            }
            return concatenatedArray;
        }

        public static bool IsEquivalentTo(this byte[] left, int leftOffset, byte[] right, int rightOffset, int length)
        {
            if (left == null)
            {
                throw new ArgumentNullException("left");
            }
            if (right == null)
            {
                throw new ArgumentNullException("right");
            }
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }
            if (leftOffset < 0)
            {
                throw new ArgumentOutOfRangeException("leftOffset");
            }
            if (leftOffset + length > left.Length)
            {
                throw new ArgumentOutOfRangeException("length");
            }
            if (rightOffset < 0)
            {
                throw new ArgumentOutOfRangeException("rightOffset");
            }
            if (rightOffset + length > right.Length)
            {
                throw new ArgumentOutOfRangeException("length");
            }
            return left.IsEquivalentToInternal(leftOffset, right, rightOffset, length);
        }

        private static bool IsEquivalentToInternal(this byte[] left, int leftOffset, byte[] right, int rightOffset, int length)
        {
            for (int i = 0; i < length; ++i)
            {
                if (left[leftOffset + i] != right[rightOffset + i])
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsEquivalentTo(this byte[] left, byte[] right)
        {
            if (left == null)
            {
                throw new ArgumentNullException("left");
            }
            if (right == null)
            {
                throw new ArgumentNullException("right");
            }
            if (right.Length != left.Length)
            {
                return false;
            }
            return left.IsEquivalentToInternal(0, right, 0, right.Length);
        }

        public static long GetLittleEndianValue(this byte[] left, int offset, int length)
        {
            long value = 0;
            while (length-- > 0)
            {
                value <<= 8;
                value |= left[offset + length];
            }
            return value;
        }

        public static long GetBigEndianValue(this byte[] left, int offset, int length)
        {
            long value = 0;
            for (int i = 0; i < length; ++i)
            {
                value <<= 8;
                value |= left[offset + i];
            }
            return value;
        }

        public static byte[] Reduce(this byte[] value, int length)
        {
            if (value.Length == 0)
            {
                return value;
            }
            if (value.Length < length)
            {
                throw new ArgumentException("Can't reduce a byte array that is already shorter than the target length.");
            }
            byte[] reduced = new byte[length];
            for (int i = 0; i < value.Length; ++i)
            {
                reduced[i % length] ^= value[i];
            }
            return reduced;
        }
    }
}
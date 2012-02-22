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
 * The source is maintained at http://AxCrypt.codeplex.com/ please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using System;
using System.Globalization;

namespace Axantum.AxCrypt.Core
{
    public static class Extensions
    {
        /// <summary>
        /// Extension for String.Format using InvariantCulture
        /// </summary>
        /// <param name="format"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string InvariantFormat(this string format, params object[] parameters)
        {
            string formatted = String.Format(CultureInfo.InvariantCulture, format, parameters);
            return formatted;
        }

        /// <summary>
        /// Convenience extension for String.Format using the provided CultureInfo
        /// </summary>
        /// <param name="format"></param>
        /// <param name="cultureInfo"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string FormatWith(this string format, CultureInfo cultureInfo, params object[] parameters)
        {
            string formatted = String.Format(cultureInfo, format, parameters);
            return formatted;
        }

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
            int candidatePosition = offset;
            while (candidatePosition - offset + pattern.Length <= count)
            {
                int i = 0;
                for (; i < pattern.Length; ++i)
                {
                    if (buffer[candidatePosition + i] != pattern[i])
                    {
                        break;
                    }
                }
                if (i == pattern.Length)
                {
                    return candidatePosition;
                }
                ++candidatePosition;
            }
            return -1;
        }

        public static void Xor(this byte[] buffer, byte[] other)
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
        }

        public static void Xor(this byte[] buffer, int bufferIndex, byte[] other, int otherIndex, int length)
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
            if (leftOffset + length > left.Length || length < 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }
            if (leftOffset < 0)
            {
                throw new ArgumentOutOfRangeException("leftOffset");
            }
            if (rightOffset + length > right.Length)
            {
                throw new ArgumentOutOfRangeException("length");
            }
            if (rightOffset < 0)
            {
                throw new ArgumentOutOfRangeException("rightOffset");
            }
            for (int i = 0; i < length; ++i)
            {
                if (left[leftOffset + i] != right[rightOffset + i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
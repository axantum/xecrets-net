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
using System.Globalization;
using System.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;

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
            return left.IsEquivalentTo(0, right, 0, right.Length);
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

        public static byte[] GetLittleEndianBytes(this long value)
        {
            if (OS.Current.IsLittleEndian)
            {
                return BitConverter.GetBytes(value);
            }

            byte[] bytes = new byte[sizeof(long)];

            for (int i = 0; value != 0 && i < bytes.Length; ++i)
            {
                bytes[i] = (byte)value;
                value >>= 8;
            }
            return bytes;
        }

        public static byte[] GetLittleEndianBytes(this int value)
        {
            if (OS.Current.IsLittleEndian)
            {
                return BitConverter.GetBytes(value);
            }

            byte[] bytes = new byte[sizeof(int)];

            for (int i = 0; value != 0 && i < bytes.Length; ++i)
            {
                bytes[i] = (byte)value;
                value >>= 8;
            }
            return bytes;
        }

        public static byte[] GetBigEndianBytes(this long value)
        {
            if (!OS.Current.IsLittleEndian)
            {
                return BitConverter.GetBytes(value);
            }

            byte[] bytes = new byte[sizeof(long)];

            for (int i = bytes.Length - 1; value != 0 && i >= 0; --i)
            {
                bytes[i] = (byte)value;
                value >>= 8;
            }
            return bytes;
        }

        /// <summary>
        /// Create a file name based on an existing, but convert the file name to the pattern used by
        /// AxCrypt for encrypted files. The original must not already be in that form.
        /// </summary>
        /// <param name="fileInfo">A file name representing a file that is not encrypted</param>
        /// <returns>A corresponding file name representing the encrypted version of the original</returns>
        /// <exception cref="InternalErrorException">Can't get encrypted name for a file that already has the encrypted extension.</exception>
        public static string CreateEncryptedName(this string fullName)
        {
            string extension = Path.GetExtension(fullName);
            if (String.Compare(extension, OS.Current.AxCryptExtension, StringComparison.OrdinalIgnoreCase) == 0)
            {
                throw new InternalErrorException("Can't get encrypted name for a file that already has the encrypted extension.");
            }
            string encryptedName = fullName;
            encryptedName = encryptedName.Substring(0, encryptedName.Length - extension.Length);
            encryptedName += extension.Replace('.', '-');
            encryptedName += OS.Current.AxCryptExtension;

            return encryptedName;
        }

        public static bool HasMask(this AxCryptOptions options, AxCryptOptions mask)
        {
            return (options & mask) == mask;
        }

        public static bool HasMask(this ActiveFileStatus status, ActiveFileStatus mask)
        {
            return (status & mask) == mask;
        }

        public static void CopyTo(this Stream source, Stream destination, int bufferSize)
        {
            if (source == null)
            {
                throw new ArgumentNullException("destination");
            }
            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }
            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException("bufferSize", "Buffer size must be greater than zero.");
            }

            byte[] buffer = new byte[bufferSize];
            int read;
            while ((read = source.Read(buffer, 0, buffer.Length)) != 0)
            {
                destination.Write(buffer, 0, read);
            }
        }

        public static string PathCombine(this string path, params string[] parts)
        {
            foreach (string part in parts)
            {
                path = Path.Combine(path, part);
            }
            return path;
        }

        /// <summary>
        /// Trim a log message from extra information in front, specifically text preceding the
        /// log level text such as Information, Warning etc. There must be a space preceding
        /// the log level text. Recognized texts are 'Information', 'Warning', 'Debug', 'Error'
        /// and 'Fatal'.
        /// </summary>
        /// <param name="message">A log message</param>
        /// <returns>A possible trimmed message</returns>
        /// <remarks>
        /// This is primarily intended to facilitate more compact logging in a GUI
        /// </remarks>
        public static string TrimLogMessage(this string message)
        {
            int skipIndex = message.IndexOf(" Information", StringComparison.Ordinal);
            skipIndex = skipIndex < 0 ? message.IndexOf(" Warning", StringComparison.Ordinal) : skipIndex;
            skipIndex = skipIndex < 0 ? message.IndexOf(" Debug", StringComparison.Ordinal) : skipIndex;
            skipIndex = skipIndex < 0 ? message.IndexOf(" Error", StringComparison.Ordinal) : skipIndex;
            skipIndex = skipIndex < 0 ? message.IndexOf(" Fatal", StringComparison.Ordinal) : skipIndex;

            return message.Substring(skipIndex + 1);
        }
    }
}
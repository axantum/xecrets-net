#region Xecrets Cli Copyright and GPL License notice

/*
 * Xecrets Cli - Changes and additions copyright © 2022-2024, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets Cli, but is derived from AxCrypt as licensed under GPL v3 or later.
 * 
 * The changes and additions are separately copyrighted and only licensed under GPL v3 or later as detailed below,
 * unless explicitly licensed otherwise. If you use any part of these changes and additions in your software,
 * please see https://www.gnu.org/licenses/ for details of what this means for you.
 * 
 * Warning: If you are using the original AxCrypt code under a non-GPL v3 or later license, these changes and additions
 * are not included in that license. If you use these changes under those circumstances, all your code becomes subject to
 * the GPL v3 or later license, according to the principle of strong copyleft as applied to GPL v3 or later.
 *
 * Xecrets Cli is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets Cli is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets Cli. If not, see
 * https://www.gnu.org/licenses/.
 *
 * The source repository can be found at https://github.com/axantum/xecrets-net please go there for more information,
 * suggestions and contributions, as well for commit history detailing changes and additions that fall under the strong
 * copyleft provisions mentioned above. You may also visit https://www.axantum.com for more information about the author.
*/

#endregion Xecrets Cli Copyright and GPL License notice
#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
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
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Abstractions.Algorithm;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Extensions
{
    public static class StreamExtensions
    {
        public static void CopyTo(this Stream source, Stream destination, int bufferSize)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }
            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize), "Buffer size must be greater than zero.");
            }

            byte[] buffer = new byte[bufferSize];
            int read;
            while ((read = source.Read(buffer, 0, buffer.Length)) != 0)
            {
                destination.Write(buffer, 0, read);
            }
        }

        public static long CopyTo(this Stream inputStream, Stream outputStream)
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException(nameof(inputStream));
            }
            if (outputStream == null)
            {
                throw new ArgumentNullException(nameof(outputStream));
            }

            long totalDone = 0;
            byte[] buffer = new byte[OS.Current.StreamBufferSize];
            int bufferWrittenCount = 0;
            int bufferRemainingCount = buffer.Length;
            while (true)
            {
                int readCount = inputStream.Read(buffer, bufferWrittenCount, bufferRemainingCount);
                bufferWrittenCount += readCount;
                bufferRemainingCount -= readCount;
                if (bufferRemainingCount > 0 && readCount > 0)
                {
                    continue;
                }
                if (bufferWrittenCount == 0)
                {
                    break;
                }
                outputStream.Write(buffer, 0, bufferWrittenCount);
                outputStream.Flush();
                totalDone += bufferWrittenCount;
                bufferWrittenCount = 0;
                bufferRemainingCount = buffer.Length;
            }
            return totalDone;
        }

        public static void DecryptTo(this Stream encryptedInputStream, Stream plaintextOutputStream, ICryptoTransform transform, bool isCompressed)
        {
            Exception? savedExceptionIfCloseCausesException = null;
            try
            {
                if (encryptedInputStream == null)
                {
                    throw new ArgumentNullException(nameof(encryptedInputStream));
                }
                if (plaintextOutputStream == null)
                {
                    throw new ArgumentNullException(nameof(plaintextOutputStream));
                }
                if (transform == null)
                {
                    throw new ArgumentNullException(nameof(transform));
                }

                if (isCompressed)
                {
                    using Stream deflatedPlaintextStream = New<CryptoStreamBase>().Initialize(encryptedInputStream, transform, CryptoStreamMode.Read);
                    using ZLibStream inflatedPlaintextStream = new(deflatedPlaintextStream, CompressionMode.Decompress);
                    try
                    {
                        inflatedPlaintextStream.CopyTo(plaintextOutputStream);
                    }
                    catch (Exception ex)
                    {
                        savedExceptionIfCloseCausesException = ex;
                        throw;
                    }
                }
                else
                {
                    using Stream plainStream = New<CryptoStreamBase>().Initialize(encryptedInputStream, transform, CryptoStreamMode.Read);
                    try
                    {
                        plainStream.CopyTo(plaintextOutputStream);
                    }
                    catch (Exception ex)
                    {
                        savedExceptionIfCloseCausesException = ex;
                        throw;
                    }
                }
            }
            catch (Exception)
            {
                if (savedExceptionIfCloseCausesException != null)
                {
                    throw savedExceptionIfCloseCausesException;
                }
                throw;
            }
        }

        public static byte[] ToArray(this Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            using MemoryStream memStream = new MemoryStream();
            stream.CopyTo(memStream);
            return memStream.ToArray();
        }

        public static bool Skip(this Stream stream, long bytesToSkip)
        {
            byte[] buffer = new byte[bytesToSkip > 1024 * 1024 ? 1024 * 1024 : bytesToSkip];
            while (bytesToSkip > 0)
            {
                int bytesRead = stream.Read(buffer, 0, (int)(buffer.Length > bytesToSkip ? bytesToSkip : buffer.Length));
                if (bytesRead <= 0)
                {
                    return false;
                }
                bytesToSkip -= bytesRead;
            }
            return true;
        }
    }
}

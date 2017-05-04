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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Abstractions.Algorithm;
using Axantum.AxCrypt.Core.Algorithm;
using Org.BouncyCastle.Utilities.Zlib;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Extensions
{
    public static class StreamExtensions
    {
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

        public static long CopyTo(this Stream inputStream, Stream outputStream)
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException("inputStream");
            }
            if (outputStream == null)
            {
                throw new ArgumentNullException("outputStream");
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

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public static void DecryptTo(this Stream encryptedInputStream, Stream plaintextOutputStream, ICryptoTransform transform, bool isCompressed)
        {
            Exception savedExceptionIfCloseCausesException = null;
            try
            {
                if (encryptedInputStream == null)
                {
                    throw new ArgumentNullException("encryptedInputStream");
                }
                if (plaintextOutputStream == null)
                {
                    throw new ArgumentNullException("plaintextOutputStream");
                }
                if (transform == null)
                {
                    throw new ArgumentNullException("transform");
                }

                if (isCompressed)
                {
                    using (Stream deflatedPlaintextStream = New<CryptoStream>().Initialize(encryptedInputStream, transform, CryptoStreamMode.Read))
                    {
                        using (Stream inflatedPlaintextStream = new ZInputStream(deflatedPlaintextStream))
                        {
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
                    }
                }
                else
                {
                    using (Stream plainStream = New<CryptoStream>().Initialize(encryptedInputStream, transform, CryptoStreamMode.Read))
                    {
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

            using (MemoryStream memStream = new MemoryStream())
            {
                stream.CopyTo(memStream);
                return memStream.ToArray();
            }
        }
    }
}
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

using Axantum.AxCrypt.Core.UI;
using System;
using System.IO;
using System.Linq;

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

        public static long CopyToWithCount(this Stream inputStream, Stream outputStream, Stream realInputStream, IProgressContext progress)
        {
            progress.NotifyLevelStart();

            if (realInputStream.CanSeek)
            {
                progress.AddTotal(realInputStream.Length - realInputStream.Position);
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
                progress.AddCount(bufferWrittenCount);
                totalDone += bufferWrittenCount;
                bufferWrittenCount = 0;
                bufferRemainingCount = buffer.Length;
            }
            progress.NotifyLevelFinished();
            return totalDone;
        }
    }
}
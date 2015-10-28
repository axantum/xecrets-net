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

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using System;

namespace Axantum.AxCrypt.Core.Header
{
    public class FileInfoEncryptedHeaderBlock : EncryptedHeaderBlock
    {
        private const int CreationTimeOffset = 0;
        private const int LastAccessTimeOffset = 8;
        private const int LastWriteTimeOffset = 16;
        private static readonly long WindowsTimeTicksStart = new DateTime(1601, 1, 1).Ticks;

        public FileInfoEncryptedHeaderBlock(byte[] dataBlock)
            : base(HeaderBlockType.FileInfo, dataBlock)
        {
        }

        public FileInfoEncryptedHeaderBlock(ICrypto headerCrypto)
            : this(new byte[0])
        {
            HeaderCrypto = headerCrypto;
        }

        private void EnsureDataBlock()
        {
            if (GetDataBlockBytesReference().Length > 0)
            {
                return;
            }
            byte[] timeBytes = GetTimeStampBytes(OS.Current.UtcNow);

            byte[] rawData = new byte[32];
            Array.Copy(timeBytes, 0, rawData, CreationTimeOffset, timeBytes.Length);
            Array.Copy(timeBytes, 0, rawData, LastAccessTimeOffset, timeBytes.Length);
            Array.Copy(timeBytes, 0, rawData, LastWriteTimeOffset, timeBytes.Length);

            SetDataBlockBytesReference(HeaderCrypto.Encrypt(rawData));
        }

        public override object Clone()
        {
            FileInfoEncryptedHeaderBlock block = new FileInfoEncryptedHeaderBlock((byte[])GetDataBlockBytesReference().Clone());
            return CopyTo(block);
        }

        public DateTime CreationTimeUtc
        {
            get
            {
                DateTime creationTime = GetTimeStamp(CreationTimeOffset);
                return creationTime;
            }
            set
            {
                SetTimeStamp(value, CreationTimeOffset);
            }
        }

        public DateTime LastAccessTimeUtc
        {
            get
            {
                DateTime lastAccessTime = GetTimeStamp(LastAccessTimeOffset);
                return lastAccessTime;
            }
            set
            {
                SetTimeStamp(value, LastAccessTimeOffset);
            }
        }

        public DateTime LastWriteTimeUtc
        {
            get
            {
                DateTime lastWriteTime = GetTimeStamp(LastWriteTimeOffset);
                return lastWriteTime;
            }
            set
            {
                SetTimeStamp(value, LastWriteTimeOffset);
            }
        }

        private DateTime GetTimeStamp(int timeOffset)
        {
            EnsureDataBlock();
            byte[] rawFileTimes = HeaderCrypto.Decrypt(GetDataBlockBytesReference());
            uint lowDateTime = (uint)rawFileTimes.GetLittleEndianValue(timeOffset, 4);
            uint hiDateTime = (uint)rawFileTimes.GetLittleEndianValue(timeOffset + 4, 4);
            long filetime = ((long)hiDateTime << 32) | lowDateTime;

            DateTime timeStampUtc = new DateTime(WindowsTimeTicksStart + filetime, DateTimeKind.Utc);
            return timeStampUtc;
        }

        private void SetTimeStamp(DateTime dateTime, int timeOffset)
        {
            EnsureDataBlock();
            byte[] timeStampBytes = GetTimeStampBytes(dateTime);
            byte[] rawFileTimes = HeaderCrypto.Decrypt(GetDataBlockBytesReference());
            Array.Copy(timeStampBytes, 0, rawFileTimes, timeOffset, timeStampBytes.Length);
            SetDataBlockBytesReference(HeaderCrypto.Encrypt(rawFileTimes));
        }

        private static byte[] GetTimeStampBytes(DateTime dateTime)
        {
            if (dateTime.Kind != DateTimeKind.Utc)
            {
                dateTime = dateTime.ToUniversalTime();
            }

            long filetime = dateTime.Ticks - WindowsTimeTicksStart;
            long lowDateTime = filetime & 0xffffffff;
            long hiDateTime = (filetime >> 32) & 0xffffffff;
            byte[] lowDateTimeBytes = lowDateTime.GetLittleEndianBytes();
            byte[] hiDateTimeBytes = hiDateTime.GetLittleEndianBytes();

            byte[] timeStampBytes = new byte[sizeof(long)];
            Array.Copy(lowDateTimeBytes, 0, timeStampBytes, 0, sizeof(uint));
            Array.Copy(hiDateTimeBytes, 0, timeStampBytes, sizeof(uint), sizeof(uint));

            return timeStampBytes;
        }
    }
}
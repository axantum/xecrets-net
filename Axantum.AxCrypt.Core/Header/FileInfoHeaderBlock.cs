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

namespace Axantum.AxCrypt.Core.Header
{
    public class FileInfoHeaderBlock : EncryptedHeaderBlock
    {
        private const int CreationTimeOffset = 0;
        private const int LastAccessTimeOffset = 8;
        private const int LastWriteTimeOffset = 16;
        private static readonly long WindowsTimeTicksStart = new DateTime(1601, 1, 1).Ticks;

        public FileInfoHeaderBlock(byte[] dataBlock)
            : base(HeaderBlockType.FileInfo, dataBlock)
        {
        }

        public DateTime GetCreationTimeUtc(AesCrypto aesCrypto)
        {
            DateTime creationTime = GetTimeStamp(CreationTimeOffset, aesCrypto);
            return creationTime;
        }

        public DateTime GetLastAccessTimeUtc(AesCrypto aesCrypto)
        {
            DateTime lastAccessTime = GetTimeStamp(LastAccessTimeOffset, aesCrypto);
            return lastAccessTime;
        }

        public DateTime GetLastWriteTimeUtc(AesCrypto aesCrypto)
        {
            DateTime lastWriteTime = GetTimeStamp(LastWriteTimeOffset, aesCrypto);
            return lastWriteTime;
        }

        private DateTime GetTimeStamp(int CreationTimeOffset, AesCrypto aesCrypto)
        {
            byte[] rawFileTimes = aesCrypto.Decrypt(GetDataBlockBytesReference());
            uint lowDateTime = (uint)rawFileTimes.GetLittleEndianValue(CreationTimeOffset, 4);
            uint hiDateTime = (uint)rawFileTimes.GetLittleEndianValue(CreationTimeOffset + 4, 4);
            long filetime = ((long)hiDateTime << 32) | lowDateTime;

            DateTime timeStampUtc = new DateTime(WindowsTimeTicksStart + filetime, DateTimeKind.Utc);
            return timeStampUtc;
        }
    }
}
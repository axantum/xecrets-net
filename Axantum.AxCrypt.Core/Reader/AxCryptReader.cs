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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.Header;
using Axantum.AxCrypt.Core.Reader;

namespace Axantum.AxCrypt.Core.Reader
{
    public class AxCryptReader : IDisposable
    {
        private const int DATA_CHUNK_SIZE = 65536;

        public static readonly Guid AxCrypt1Guid = new Guid("2e07b9c0-934f-46f1-a015-792ca1d9e821");

        private static readonly byte[] _axCrypt1GuidAsBytes = AxCrypt1Guid.ToByteArray();

        public static byte[] GetAxCrypt1GuidAsBytes()
        {
            return _axCrypt1GuidAsBytes;
        }

        private byte[] _dataChunk;
        public byte[] GetAndOwnDataChunk()
        {
            byte[] dataChunk = _dataChunk;
            _dataChunk = null;
            return dataChunk;
        }

        private Int64 _dataBytesLeftToRead;

        private LookAheadStream InputStream { get; set; }

        private bool Disposed { get; set; }

        public AxCryptReader(Stream inputStream)
        {
            InputStream = new LookAheadStream(inputStream);
            ItemType = AxCryptItemType.None;
        }

        /// <summary>
        /// Gets the type of the current item
        /// </summary>
        public AxCryptItemType ItemType { get; private set; }

        public HeaderBlock HeaderBlock { get; private set; }

        /// <summary>
        /// Read the next item from the stream
        /// </summary>
        /// <returns>true if there was a next item read.</returns>
        public bool Read()
        {
            if (ItemType == AxCryptItemType.HeaderBlock)
            {
                DataHeaderBlock dataHeaderBlock = HeaderBlock as DataHeaderBlock;
                if (dataHeaderBlock != null)
                {
                    ItemType = AxCryptItemType.Data;
                    _dataBytesLeftToRead = dataHeaderBlock.DataLength;
                }
            }

            switch (ItemType)
            {
                case AxCryptItemType.None:
                    return LookForMagicGuid();
                case AxCryptItemType.MagicGuid:
                    return LookForHeaderBlock();
                case AxCryptItemType.HeaderBlock:
                    return LookForHeaderBlock();
                case AxCryptItemType.Data:
                    return LookForData();
                case AxCryptItemType.EndOfStream:
                    return false;
                default:
                    throw new InvalidOperationException("Unexpected AxCryptItemType");
            }
        }

        private bool LookForMagicGuid()
        {
            byte[] buffer = new byte[4096];
            while (true)
            {
                int bytesRead = InputStream.Read(buffer, 0, buffer.Length);
                if (bytesRead < _axCrypt1GuidAsBytes.Length)
                {
                    InputStream.Pushback(buffer, 0, bytesRead);
                    return false;
                }

                int i = buffer.Locate(_axCrypt1GuidAsBytes, 0, bytesRead);
                if (i < 0)
                {
                    int n = bytesRead - _axCrypt1GuidAsBytes.Length + 1;
                    if (n < 0)
                    {
                        n = bytesRead;
                    }
                    InputStream.Pushback(buffer, 0, n);
                    continue;
                }
                int pos = i + _axCrypt1GuidAsBytes.Length;
                InputStream.Pushback(buffer, pos, bytesRead - pos);
                ItemType = AxCryptItemType.MagicGuid;
                return true;
            }
        }

        private bool LookForData()
        {
            int bytesToRead = _dataBytesLeftToRead > DATA_CHUNK_SIZE ? DATA_CHUNK_SIZE : (int)_dataBytesLeftToRead;
            if (bytesToRead == 0)
            {
                ItemType = AxCryptItemType.EndOfStream;
                return true;
            }

            _dataChunk = new byte[bytesToRead];
            int bytesRead = InputStream.Read(_dataChunk, 0, bytesToRead);
            if (bytesRead != bytesToRead)
            {
                throw new InvalidOperationException("Data stream truncated too short");
            }
            _dataBytesLeftToRead -= bytesRead;
            return true;
        }

        private bool LookForHeaderBlock()
        {
            byte[] lengthBytes = new byte[sizeof(Int32)];
            if (!InputStream.ReadExact(lengthBytes))
            {
                return false;
            }
            Int32 headerBlockLength = BitConverter.ToInt32(lengthBytes, 0) - 5;
            if (headerBlockLength < 0 || headerBlockLength > 0xfffff)
            {
                throw new InvalidOperationException("Invalid headerBlockLength {0}".FormatWith(headerBlockLength));
            }

            int blockType = InputStream.ReadByte();
            if (blockType < 0)
            {
                return false;
            }
            HeaderBlockType headerBlockType = (HeaderBlockType)blockType;

            byte[] dataBLock = new byte[headerBlockLength];
            if (!InputStream.ReadExact(dataBLock))
            {
                return false;
            }

            return ParseHeaderBlock(headerBlockType, dataBLock);
        }

        private bool ParseHeaderBlock(HeaderBlockType headerBlockType, byte[] dataBlock)
        {
            ItemType = AxCryptItemType.HeaderBlock;
            switch (headerBlockType)
            {
                case HeaderBlockType.None:
                    break;
                case HeaderBlockType.Any:
                    break;
                case HeaderBlockType.Preamble:
                    break;
                case HeaderBlockType.Version:
                    HeaderBlock = new VersionHeaderBlock(headerBlockType, dataBlock);
                    break;
                case HeaderBlockType.KeyWrap1:
                    break;
                case HeaderBlockType.KeyWrap2:
                    break;
                case HeaderBlockType.IdTag:
                    break;
                case HeaderBlockType.Data:
                    DataHeaderBlock dataHeaderBlock = new DataHeaderBlock(headerBlockType, dataBlock);
                    HeaderBlock = dataHeaderBlock;
                    break;
                case HeaderBlockType.Encrypted:
                    break;
                case HeaderBlockType.FileNameInfo:
                    break;
                case HeaderBlockType.EncryptionInfo:
                    break;
                case HeaderBlockType.CompressionInfo:
                    break;
                case HeaderBlockType.FileInfo:
                    break;
                case HeaderBlockType.Compression:
                    break;
                case HeaderBlockType.UnicodeFileNameInfo:
                    break;
                default:
                    return false;
            }
            return true;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }
            if (disposing)
            {
                if (InputStream != null)
                {
                    InputStream.Dispose();
                    InputStream = null;
                }
                Disposed = true;
            }
        }

        #endregion IDisposable Members
    }
}
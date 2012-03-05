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
using System.IO;
using System.Security.Cryptography;
using Axantum.AxCrypt.Core.Header;

namespace Axantum.AxCrypt.Core.Reader
{
    public abstract class AxCryptReader : IDisposable
    {
        private bool _sendDataToHmacStream = false;

        private Int64 _dataBytesLeftToRead;

        protected LookAheadStream InputStream { get; set; }

        public AxCryptReaderSettings Settings { get; set; }

        private bool Disposed { get; set; }

        public static AxCryptReader Create(Stream inputStream)
        {
            AxCryptReader reader = Create(inputStream, new AxCryptReaderSettings());

            return reader;
        }

        public static AxCryptReader Create(Stream inputStream, AxCryptReaderSettings settings)
        {
            AxCryptReader reader = new AxCryptStreamReader(inputStream);
            reader.ItemType = AxCryptItemType.None;
            reader.Settings = settings;

            return reader;
        }

        /// <summary>
        /// Gets the type of the current item
        /// </summary>
        public AxCryptItemType ItemType { get; private set; }

        public HeaderBlock HeaderBlock { get; private set; }

        public Stream CreateEncryptedDataStream()
        {
            if (ItemType != AxCryptItemType.Data)
            {
                throw new InvalidOperationException("Called out of sequence, expecting Data.");
            }
            return new AxCryptDataStream(InputStream, _sendDataToHmacStream ? Settings.HmacStream : null, _dataBytesLeftToRead);
        }

        /// <summary>
        /// Read the next item from the stream
        /// </summary>
        /// <returns>true if there was a next item read.</returns>
        public bool Read()
        {
            if (ItemType == AxCryptItemType.EndOfStream)
            {
                return false;
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
                    ItemType = AxCryptItemType.EndOfStream;
                    return true;
                case AxCryptItemType.EndOfStream:
                    return true;
                default:
                    throw new FileFormatException("Unexpected AxCryptItemType");
            }
        }

        private static byte[] _axCrypt1GuidBytes = AxCrypt1Guid.GetBytes();

        private bool LookForMagicGuid()
        {
            byte[] buffer = new byte[4096];
            while (true)
            {
                int bytesRead = InputStream.Read(buffer, 0, buffer.Length);
                if (bytesRead < AxCrypt1Guid.Length)
                {
                    InputStream.Pushback(buffer, 0, bytesRead);
                    return false;
                }

                int i = buffer.Locate(_axCrypt1GuidBytes, 0, bytesRead);
                if (i < 0)
                {
                    int offsetToBytesToKeep = bytesRead - AxCrypt1Guid.Length + 1;
                    InputStream.Pushback(buffer, offsetToBytesToKeep, bytesRead - offsetToBytesToKeep);
                    continue;
                }
                int offsetJustAfterTheGuid = i + AxCrypt1Guid.Length;
                InputStream.Pushback(buffer, offsetJustAfterTheGuid, bytesRead - offsetJustAfterTheGuid);
                ItemType = AxCryptItemType.MagicGuid;
                return true;
            }
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
                throw new FileFormatException("Invalid headerBlockLength {0}".InvariantFormat(headerBlockLength));
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

            if (!ParseHeaderBlock(headerBlockType, dataBLock))
            {
                return false;
            }

            DataHeaderBlock dataHeaderBlock = HeaderBlock as DataHeaderBlock;
            if (dataHeaderBlock != null)
            {
                ItemType = AxCryptItemType.Data;
                _dataBytesLeftToRead = dataHeaderBlock.DataLength;
            }

            return true;
        }

        private bool ParseHeaderBlock(HeaderBlockType headerBlockType, byte[] dataBlock)
        {
            bool isFirst = ItemType == AxCryptItemType.MagicGuid;
            ItemType = AxCryptItemType.HeaderBlock;
            if (isFirst && headerBlockType != HeaderBlockType.Preamble)
            {
                throw new FileFormatException("Preamble must be first.");
            }
            switch (headerBlockType)
            {
                case HeaderBlockType.Preamble:
                    if (!isFirst)
                    {
                        throw new FileFormatException("Preamble can only be first.");
                    }
                    HeaderBlock = new PreambleHeaderBlock(dataBlock);
                    if (Settings.HmacStream != null)
                    {
                        _sendDataToHmacStream = true;
                    }
                    break;
                case HeaderBlockType.Version:
                    HeaderBlock = new VersionHeaderBlock(dataBlock);
                    break;
                case HeaderBlockType.KeyWrap1:
                    HeaderBlock = new KeyWrap1HeaderBlock(dataBlock);
                    break;
                case HeaderBlockType.KeyWrap2:
                    HeaderBlock = new KeyWrap2HeaderBlock(dataBlock);
                    break;
                case HeaderBlockType.IdTag:
                    HeaderBlock = new IdTagHeaderBlock(dataBlock);
                    break;
                case HeaderBlockType.Data:
                    HeaderBlock = new DataHeaderBlock(dataBlock);
                    break;
                case HeaderBlockType.Encrypted:
                    break;
                case HeaderBlockType.FileNameInfo:
                    HeaderBlock = new FileNameInfoHeaderBlock(dataBlock);
                    break;
                case HeaderBlockType.EncryptionInfo:
                    HeaderBlock = new EncryptionInfoHeaderBlock(dataBlock);
                    break;
                case HeaderBlockType.CompressionInfo:
                    HeaderBlock = new CompressionInfoHeaderBlock(dataBlock);
                    break;
                case HeaderBlockType.FileInfo:
                    HeaderBlock = new FileInfoHeaderBlock(dataBlock);
                    break;
                case HeaderBlockType.Compression:
                    HeaderBlock = new CompressionHeaderBlock(dataBlock);
                    break;
                case HeaderBlockType.UnicodeFileNameInfo:
                    HeaderBlock = new UnicodeFileNameInfoHeaderBlock(dataBlock);
                    break;
                case HeaderBlockType.None:
                case HeaderBlockType.Any:
                    return false;
                default:
                    HeaderBlock = new UnrecognizedHeaderBlock(dataBlock);
                    break;
            }

            if (_sendDataToHmacStream)
            {
                HeaderBlock.Write(Settings.HmacStream);
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
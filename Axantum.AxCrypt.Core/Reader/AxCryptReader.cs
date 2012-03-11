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
using System.IO;
using System.Security.Cryptography;
using Axantum.AxCrypt.Core.Header;

namespace Axantum.AxCrypt.Core.Reader
{
    public abstract class AxCryptReader : IDisposable
    {
        private bool _sendDataToHmacStream = false;

        private MemoryStream _hmacBufferStream = new MemoryStream();

        private Int64 _dataBytesLeftToRead;

        private LookAheadStream _inputStream;

        public AxCryptReaderSettings Settings { get; set; }

        private bool _disposed;

        public static AxCryptReader Create(Stream inputStream)
        {
            AxCryptReader reader = Create(inputStream, new AxCryptReaderSettings());

            return reader;
        }

        public static AxCryptReader Create(Stream inputStream, AxCryptReaderSettings settings)
        {
            AxCryptReader reader = new AxCryptStreamReader(inputStream);
            reader.CurrentItemType = AxCryptItemType.None;
            reader.Settings = settings;

            return reader;
        }

        /// <summary>
        /// Gets the type of the current item
        /// </summary>
        public AxCryptItemType CurrentItemType { get; private set; }

        public HeaderBlock CurrentHeaderBlock { get; private set; }

        public Stream CreateEncryptedDataStream(Stream hmacStream)
        {
            if (CurrentItemType != AxCryptItemType.Data)
            {
                throw new InvalidOperationException("Called out of sequence, expecting Data.");
            }
            if (_hmacBufferStream == null)
            {
                throw new InvalidOperationException("Can only be called once.");
            }
            if (hmacStream != null)
            {
                _hmacBufferStream.Position = 0;
                _hmacBufferStream.CopyTo(hmacStream);
            }
            _hmacBufferStream.Dispose();
            _hmacBufferStream = null;
            return new AxCryptDataStream(_inputStream, hmacStream, _dataBytesLeftToRead);
        }

        /// <summary>
        /// Read the next item from the stream
        /// </summary>
        /// <returns>true if there was a next item read.</returns>
        public bool Read()
        {
            if (CurrentItemType == AxCryptItemType.EndOfStream)
            {
                return false;
            }
            bool expectedItemWasFound = false;
            switch (CurrentItemType)
            {
                case AxCryptItemType.None:
                    expectedItemWasFound = LookForMagicGuid();
                    break;
                case AxCryptItemType.MagicGuid:
                case AxCryptItemType.HeaderBlock:
                    expectedItemWasFound = LookForHeaderBlock();
                    break;
                case AxCryptItemType.Data:
                    CurrentItemType = AxCryptItemType.EndOfStream;
                    return true;
                case AxCryptItemType.EndOfStream:
                    return true;
                default:
                    throw new InternalErrorException("An AxCryptItemType that should not be possible to get was found.");
            }
            if (expectedItemWasFound)
            {
                return true;
            }
            CurrentItemType = AxCryptItemType.None;
            return false;
        }

        protected void SetInputStream(LookAheadStream inputStream)
        {
            _inputStream = inputStream;
        }

        private static byte[] _axCrypt1GuidBytes = AxCrypt1Guid.GetBytes();

        private bool LookForMagicGuid()
        {
            byte[] buffer = new byte[4096];
            while (true)
            {
                int bytesRead = _inputStream.Read(buffer, 0, buffer.Length);
                if (bytesRead < AxCrypt1Guid.Length)
                {
                    _inputStream.Pushback(buffer, 0, bytesRead);
                    return false;
                }

                int i = buffer.Locate(_axCrypt1GuidBytes, 0, bytesRead);
                if (i < 0)
                {
                    int offsetToBytesToKeep = bytesRead - AxCrypt1Guid.Length + 1;
                    _inputStream.Pushback(buffer, offsetToBytesToKeep, bytesRead - offsetToBytesToKeep);
                    continue;
                }
                int offsetJustAfterTheGuid = i + AxCrypt1Guid.Length;
                _inputStream.Pushback(buffer, offsetJustAfterTheGuid, bytesRead - offsetJustAfterTheGuid);
                CurrentItemType = AxCryptItemType.MagicGuid;
                return true;
            }
        }

        private bool LookForHeaderBlock()
        {
            byte[] lengthBytes = new byte[sizeof(Int32)];
            if (!_inputStream.ReadExact(lengthBytes))
            {
                return false;
            }
            Int32 headerBlockLength = BitConverter.ToInt32(lengthBytes, 0) - 5;
            if (headerBlockLength < 0 || headerBlockLength > 0xfffff)
            {
                throw new FileFormatException("Invalid headerBlockLength {0}".InvariantFormat(headerBlockLength));
            }

            int blockType = _inputStream.ReadByte();
            if (blockType < 0)
            {
                return false;
            }
            HeaderBlockType headerBlockType = (HeaderBlockType)blockType;

            byte[] dataBLock = new byte[headerBlockLength];
            if (!_inputStream.ReadExact(dataBLock))
            {
                return false;
            }

            if (!ParseHeaderBlock(headerBlockType, dataBLock))
            {
                return false;
            }

            DataHeaderBlock dataHeaderBlock = CurrentHeaderBlock as DataHeaderBlock;
            if (dataHeaderBlock != null)
            {
                CurrentItemType = AxCryptItemType.Data;
                _dataBytesLeftToRead = dataHeaderBlock.DataLength;
            }

            return true;
        }

        private bool ParseHeaderBlock(HeaderBlockType headerBlockType, byte[] dataBlock)
        {
            bool isFirst = CurrentItemType == AxCryptItemType.MagicGuid;
            CurrentItemType = AxCryptItemType.HeaderBlock;

            if (headerBlockType == HeaderBlockType.Preamble)
            {
                if (!isFirst)
                {
                    throw new FileFormatException("Preamble can only be first.");
                }
                CurrentHeaderBlock = new PreambleHeaderBlock(dataBlock);
                _sendDataToHmacStream = true;
                return true;
            }
            else
            {
                if (isFirst)
                {
                    throw new FileFormatException("Preamble must be first.");
                }
            }

            switch (headerBlockType)
            {
                case HeaderBlockType.Version:
                    CurrentHeaderBlock = new VersionHeaderBlock(dataBlock);
                    break;
                case HeaderBlockType.KeyWrap1:
                    CurrentHeaderBlock = new KeyWrap1HeaderBlock(dataBlock);
                    break;
                case HeaderBlockType.KeyWrap2:
                    CurrentHeaderBlock = new KeyWrap2HeaderBlock(dataBlock);
                    break;
                case HeaderBlockType.IdTag:
                    CurrentHeaderBlock = new IdTagHeaderBlock(dataBlock);
                    break;
                case HeaderBlockType.Data:
                    CurrentHeaderBlock = new DataHeaderBlock(dataBlock);
                    break;
                case HeaderBlockType.Encrypted:
                    break;
                case HeaderBlockType.FileNameInfo:
                    CurrentHeaderBlock = new FileNameInfoHeaderBlock(dataBlock);
                    break;
                case HeaderBlockType.EncryptionInfo:
                    CurrentHeaderBlock = new EncryptionInfoHeaderBlock(dataBlock);
                    break;
                case HeaderBlockType.CompressionInfo:
                    CurrentHeaderBlock = new CompressionInfoHeaderBlock(dataBlock);
                    break;
                case HeaderBlockType.FileInfo:
                    CurrentHeaderBlock = new FileInfoHeaderBlock(dataBlock);
                    break;
                case HeaderBlockType.Compression:
                    CurrentHeaderBlock = new CompressionHeaderBlock(dataBlock);
                    break;
                case HeaderBlockType.UnicodeFileNameInfo:
                    CurrentHeaderBlock = new UnicodeFileNameInfoHeaderBlock(dataBlock);
                    break;
                case HeaderBlockType.None:
                case HeaderBlockType.Any:
                    return false;
                default:
                    CurrentHeaderBlock = new UnrecognizedHeaderBlock(dataBlock);
                    break;
            }

            if (_sendDataToHmacStream)
            {
                CurrentHeaderBlock.Write(_hmacBufferStream);
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
            if (_disposed)
            {
                return;
            }
            if (disposing)
            {
                if (_inputStream != null)
                {
                    _inputStream.Dispose();
                    _inputStream = null;
                }
                if (_hmacBufferStream != null)
                {
                    _hmacBufferStream.Dispose();
                    _hmacBufferStream = null;
                }
                _disposed = true;
            }
        }

        #endregion IDisposable Members
    }
}
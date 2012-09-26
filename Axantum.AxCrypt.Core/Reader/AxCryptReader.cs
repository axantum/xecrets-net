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
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.System;
using Axantum.AxCrypt.Core.UI;

namespace Axantum.AxCrypt.Core.Reader
{
    public abstract class AxCryptReader : IDisposable
    {
        private bool _sendDataToHmacStream = false;

        private MemoryStream _hmacBufferStream = new MemoryStream();

        private long _expectedTotalHmacLength = 0;

        private LookAheadStream _inputStream;

        private bool _disposed;

        /// <summary>
        /// Instantiate an AxCryptReader from a stream.
        /// </summary>
        /// <param name="inputStream">The stream to read from, will be disposed when this instance is disposed.</param>
        /// <returns></returns>
        public static AxCryptReader Create(Stream inputStream)
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException("inputStream");
            }
            AxCryptReader reader = new AxCryptStreamReader(inputStream);
            reader.CurrentItemType = AxCryptItemType.None;

            return reader;
        }

        /// <summary>
        /// Gets the type of the current item
        /// </summary>
        public AxCryptItemType CurrentItemType { get; protected set; }

        public HeaderBlock CurrentHeaderBlock { get; private set; }

        private HmacStream _hmacStream;

        public AxCryptDataStream CreateEncryptedDataStream(AesKey hmacKey, long cipherTextLength, ProgressContext progress)
        {
            if (hmacKey == null)
            {
                throw new ArgumentNullException("hmacKey");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            if (CurrentItemType != AxCryptItemType.Data)
            {
                throw new InvalidOperationException("GetEncryptedDataStream() was called when the reader is not positioned at the data.");
            }

            CurrentItemType = AxCryptItemType.EndOfStream;

            _hmacStream = new HmacStream(hmacKey);
            _hmacBufferStream.Position = 0;
            _hmacBufferStream.CopyTo(_hmacStream, Os.Current.StreamBufferSize);

            _expectedTotalHmacLength = _hmacBufferStream.Length + cipherTextLength;

            AxCryptDataStream encryptedDataStream = new AxCryptDataStream(_inputStream, _hmacStream, cipherTextLength);

            return encryptedDataStream;
        }

        public DataHmac Hmac
        {
            get
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
                if (CurrentItemType != AxCryptItemType.EndOfStream)
                {
                    throw new InvalidOperationException("There is no valid HMAC until the encrypted data stream is read to end.");
                }
                if (_hmacStream.Length != _expectedTotalHmacLength)
                {
                    throw new InvalidOperationException("There is no valid HMAC until the encrypted data stream is read to end.");
                }
                return _hmacStream.HmacResult;
            }
        }

        /// <summary>
        /// Read the next item from the stream.
        /// </summary>
        /// <returns>true if there was a next item read, false if at end of stream.</returns>
        /// <exception cref="Axantum.AxCrypt.Core.AxCryptException">Any error except premature end of stream will throw.</exception>
        public virtual bool Read()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            AxCryptItemType before = CurrentItemType;
            bool readOk = ReadInternal();
            AxCryptItemType after = CurrentItemType;
            if (Os.Log.IsDebugEnabled)
            {
                Os.Log.Debug("AxCryptReader.Read() from type {0} to type {1} : {2}.".InvariantFormat(before, after, CurrentHeaderBlock == null ? "(None)" : CurrentHeaderBlock.GetType().ToString()));
            }
            return readOk;
        }

        private bool ReadInternal()
        {
            switch (CurrentItemType)
            {
                case AxCryptItemType.None:
                    LookForMagicGuid();
                    return CurrentItemType != AxCryptItemType.EndOfStream;
                case AxCryptItemType.MagicGuid:
                case AxCryptItemType.HeaderBlock:
                    LookForHeaderBlock();
                    return CurrentItemType != AxCryptItemType.EndOfStream;
                case AxCryptItemType.Data:
                    CurrentItemType = AxCryptItemType.EndOfStream;
                    return false;
                case AxCryptItemType.EndOfStream:
                    return false;
                default:
                    throw new InternalErrorException("An AxCryptItemType that should not be possible to get was found.");
            }
        }

        protected void SetInputStream(LookAheadStream inputStream)
        {
            _inputStream = inputStream;
        }

        private static byte[] _axCrypt1GuidBytes = AxCrypt1Guid.GetBytes();

        private void LookForMagicGuid()
        {
            byte[] buffer = new byte[Os.Current.StreamBufferSize];
            while (true)
            {
                int bytesRead = _inputStream.Read(buffer, 0, buffer.Length);
                if (bytesRead < AxCrypt1Guid.Length)
                {
                    _inputStream.Pushback(buffer, 0, bytesRead);
                    CurrentItemType = AxCryptItemType.EndOfStream;
                    return;
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
                return;
            }
        }

        private void LookForHeaderBlock()
        {
            byte[] lengthBytes = new byte[sizeof(Int32)];
            if (!_inputStream.ReadExact(lengthBytes))
            {
                CurrentItemType = AxCryptItemType.EndOfStream;
                return;
            }
            Int32 headerBlockLength = BitConverter.ToInt32(lengthBytes, 0) - 5;
            if (headerBlockLength < 0 || headerBlockLength > 0xfffff)
            {
                throw new FileFormatException("Invalid headerBlockLength {0}".InvariantFormat(headerBlockLength), ErrorStatus.FileFormatError);
            }

            int blockType = _inputStream.ReadByte();
            if (blockType > 127)
            {
                throw new FileFormatException("Invalid block type {0}".InvariantFormat(blockType), ErrorStatus.FileFormatError);
            }
            HeaderBlockType headerBlockType = (HeaderBlockType)blockType;

            byte[] dataBLock = new byte[headerBlockLength];
            if (!_inputStream.ReadExact(dataBLock))
            {
                CurrentItemType = AxCryptItemType.EndOfStream;
                return;
            }

            ParseHeaderBlock(headerBlockType, dataBLock);

            DataHeaderBlock dataHeaderBlock = CurrentHeaderBlock as DataHeaderBlock;
            if (dataHeaderBlock != null)
            {
                CurrentItemType = AxCryptItemType.Data;
            }
        }

        private void ParseHeaderBlock(HeaderBlockType headerBlockType, byte[] dataBlock)
        {
            bool isFirst = CurrentItemType == AxCryptItemType.MagicGuid;
            CurrentItemType = AxCryptItemType.HeaderBlock;

            if (headerBlockType == HeaderBlockType.Preamble)
            {
                if (!isFirst)
                {
                    throw new FileFormatException("Preamble can only be first.", ErrorStatus.FileFormatError);
                }
                CurrentHeaderBlock = new PreambleHeaderBlock(dataBlock);
                _sendDataToHmacStream = true;
                return;
            }
            else
            {
                if (isFirst)
                {
                    throw new FileFormatException("Preamble must be first.", ErrorStatus.FileFormatError);
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

                case HeaderBlockType.Encrypted:
                case HeaderBlockType.None:
                case HeaderBlockType.Any:
                    throw new FileFormatException("Illegal header block type.", ErrorStatus.FileFormatError);
                default:
                    CurrentHeaderBlock = new UnrecognizedHeaderBlock(headerBlockType, dataBlock);
                    break;
            }

            if (_sendDataToHmacStream)
            {
                CurrentHeaderBlock.Write(_hmacBufferStream);
            }

            return;
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
                if (_hmacStream != null)
                {
                    _hmacStream.Dispose();
                    _hmacStream = null;
                }
                _disposed = true;
            }
        }

        #endregion IDisposable Members
    }
}
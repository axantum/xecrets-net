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

using System;
using System.Collections.Generic;
using System.IO;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Header;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;

namespace Axantum.AxCrypt.Core.Reader
{
    public abstract class AxCryptReader : IDisposable
    {
        public LookAheadStream InputStream { get; set; }

        /// <summary>
        /// Implement an AxCryptReader based on a Stream.
        /// </summary>
        /// <param name="inputStream">The stream. Will be disposed when this instance is disposed.</param>
        protected AxCryptReader(Stream inputStream)
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException("inputStream");
            }
            LookAheadStream lookAheadStream = inputStream as LookAheadStream;
            if (lookAheadStream == null)
            {
                lookAheadStream = new LookAheadStream(inputStream);
            }
            InputStream = lookAheadStream;
        }

        public virtual void Reinterpret(IList<HeaderBlock> inputHeaders, IList<HeaderBlock> outputHeaders)
        {
            outputHeaders.Clear();
            foreach (HeaderBlock header in inputHeaders)
            {
                outputHeaders.Add(HeaderBlockFactory(header.HeaderBlockType, header.GetDataBlockBytes()));
            }
            CurrentItemType = AxCryptItemType.Data;
        }

        public abstract ICrypto Crypto(Headers headers, string passphrase, Guid cryptoId);

        public abstract IAxCryptDocument Document(IPassphrase key, Headers headers);

        /// <summary>
        /// Gets the type of the current item
        /// </summary>
        public virtual AxCryptItemType CurrentItemType { get; protected set; }

        public HeaderBlock CurrentHeaderBlock { get; private set; }

        protected abstract HeaderBlock HeaderBlockFactory(HeaderBlockType headerBlockType, byte[] dataBlock);

        /// <summary>
        /// Read the next item from the stream.
        /// </summary>
        /// <returns>true if there was a next item read, false if at end of stream.</returns>
        /// <exception cref="Axantum.AxCrypt.Core.AxCryptException">Any error except premature end of stream will throw.</exception>
        public virtual bool Read()
        {
            if (InputStream == null)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            AxCryptItemType before = CurrentItemType;
            bool readOk = ReadInternal();
            AxCryptItemType after = CurrentItemType;
            if (Instance.Log.IsDebugEnabled)
            {
                Instance.Log.LogDebug("AxCryptReader.Read() from type {0} to type {1} : {2}.".InvariantFormat(before, after, CurrentHeaderBlock == null ? "(None)" : CurrentHeaderBlock.GetType().ToString()));
            }
            return readOk;
        }

        public void SetEndOfStream()
        {
            CurrentItemType = AxCryptItemType.EndOfStream;
        }

        public void SetStartOfData()
        {
            CurrentItemType = AxCryptItemType.HeaderBlock;
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
                    return false;

                case AxCryptItemType.EndOfStream:
                    return false;

                default:
                    throw new InternalErrorException("An AxCryptItemType that should not be possible to get was found.");
            }
        }

        private static readonly byte[] _axCrypt1GuidBytes = AxCrypt1Guid.GetBytes();

        private void LookForMagicGuid()
        {
            byte[] buffer = new byte[OS.Current.StreamBufferSize];
            while (true)
            {
                int bytesRead = InputStream.Read(buffer, 0, buffer.Length);
                if (bytesRead < AxCrypt1Guid.Length)
                {
                    InputStream.Pushback(buffer, 0, bytesRead);
                    CurrentItemType = AxCryptItemType.EndOfStream;
                    return;
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
                CurrentItemType = AxCryptItemType.MagicGuid;
                return;
            }
        }

        private void LookForHeaderBlock()
        {
            byte[] lengthBytes = new byte[sizeof(Int32)];
            if (!InputStream.ReadExact(lengthBytes))
            {
                CurrentItemType = AxCryptItemType.EndOfStream;
                return;
            }
            Int32 headerBlockLength = BitConverter.ToInt32(lengthBytes, 0) - 5;
            if (headerBlockLength < 0 || headerBlockLength > 0xfffff)
            {
                throw new FileFormatException("Invalid headerBlockLength {0}".InvariantFormat(headerBlockLength), ErrorStatus.FileFormatError);
            }

            int blockType = InputStream.ReadByte();
            if (blockType > 127)
            {
                throw new FileFormatException("Invalid block type {0}".InvariantFormat(blockType), ErrorStatus.FileFormatError);
            }
            HeaderBlockType headerBlockType = (HeaderBlockType)blockType;

            byte[] dataBlock = new byte[headerBlockLength];
            if (!InputStream.ReadExact(dataBlock))
            {
                CurrentItemType = AxCryptItemType.EndOfStream;
                return;
            }

            ParseHeaderBlock(headerBlockType, dataBlock);

            DataHeaderBlock dataHeaderBlock = CurrentHeaderBlock as DataHeaderBlock;
            if (dataHeaderBlock != null)
            {
                CurrentItemType = AxCryptItemType.Data;
            }
        }

        private void ParseHeaderBlock(HeaderBlockType headerBlockType, byte[] dataBlock)
        {
            bool isFirst = CurrentItemType == AxCryptItemType.MagicGuid;
            switch (headerBlockType)
            {
                case HeaderBlockType.Preamble:
                    if (!isFirst)
                    {
                        throw new FileFormatException("Preamble can only be first.", ErrorStatus.FileFormatError);
                    }
                    break;

                case HeaderBlockType.Encrypted:
                case HeaderBlockType.None:
                case HeaderBlockType.Any:
                    throw new FileFormatException("Illegal header block type.", ErrorStatus.FileFormatError);
                default:
                    if (isFirst)
                    {
                        throw new FileFormatException("Preamble must be first.", ErrorStatus.FileFormatError);
                    }
                    break;
            }

            CurrentItemType = AxCryptItemType.HeaderBlock;
            CurrentHeaderBlock = HeaderBlockFactory(headerBlockType, dataBlock);
            return;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeInternal();
            }
        }

        private void DisposeInternal()
        {
            if (InputStream != null)
            {
                InputStream.Dispose();
                InputStream = null;
            }
        }

        #endregion IDisposable Members
    }
}
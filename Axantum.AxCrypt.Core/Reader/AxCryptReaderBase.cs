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

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Header;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;

namespace Axantum.AxCrypt.Core.Reader
{
    public abstract class AxCryptReaderBase
    {
        public LookAheadStream InputStream { get; set; }

        public static IAxCryptDocument Document(AxCryptReaderBase reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            return reader.Document();
        }

        /// <summary>
        /// Implement an AxCryptReader based on a Stream.
        /// </summary>
        /// <param name="inputStream">The stream. Will be disposed when this instance is disposed.</param>
        protected AxCryptReaderBase(LookAheadStream inputStream)
            : this()
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException("inputStream");
            }
            InputStream = inputStream;
        }

        private bool _referencedByDocumentInstance;

        protected AxCryptReaderBase()
        {
            _referencedByDocumentInstance = false;
        }

        public virtual void Reinterpret(IList<HeaderBlock> inputHeaders, IList<HeaderBlock> outputHeaders)
        {
            if (inputHeaders == null)
            {
                throw new ArgumentNullException("inputHeaders");
            }
            if (outputHeaders == null)
            {
                throw new ArgumentNullException("outputHeaders");
            }

            outputHeaders.Clear();
            foreach (HeaderBlock header in inputHeaders)
            {
                outputHeaders.Add(HeaderBlockFactory(header.HeaderBlockType, header.GetDataBlockBytes()));
            }
            CurrentItemType = AxCryptItemType.Data;
        }

        /// <summary>
        /// Opens an AxCrypt document instance by way of a symmetrical key and algorithm, if possible.
        /// </summary>
        /// <param name="passphrase">The key.</param>
        /// <param name="cryptoId">The crypto identifier.</param>
        /// <param name="headers">The headers.</param>
        /// <returns>An instance with a valid passphrase or not.</returns>
        public abstract IAxCryptDocument Document(Passphrase passphrase, Guid cryptoId, Headers headers);

        /// <summary>
        /// Opens an AxCrypt document instance by way of a asymmetrical private key and algorithm, if possible.
        /// </summary>
        /// <param name="privateKey">The private key.</param>
        /// <param name="cryptoId">The crypto identifier.</param>
        /// <param name="headers">The headers.</param>
        /// <returns>An instance with a valid passphrase or not.</returns>
        public abstract IAxCryptDocument Document(IAsymmetricPrivateKey privateKey, Guid cryptoId, Headers headers);

        protected virtual IAxCryptDocument Document()
        {
            if (_referencedByDocumentInstance)
            {
                throw new InvalidOperationException("A single reader instance can only be referenced by a single document instance.");
            }
            _referencedByDocumentInstance = true;
            return null;
        }

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
            if (Resolve.Log.IsDebugEnabled)
            {
                Resolve.Log.LogDebug("AxCryptReader.Read() from type {0} to type {1} : {2}.".InvariantFormat(before, after, CurrentHeaderBlock == null ? "(None)" : CurrentHeaderBlock.GetType().ToString()));
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
                    throw new InternalErrorException("An item type that should not be possible to get was found.");
            }
        }

        private static readonly byte[] _axCrypt1GuidBytes = AxCrypt1Guid.GetBytes();

        private void LookForMagicGuid()
        {
            CurrentItemType = InputStream.Locate(_axCrypt1GuidBytes) ? AxCryptItemType.MagicGuid : AxCryptItemType.EndOfStream;
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
                throw new FileFormatException("Invalid headerBlockLength {0}".InvariantFormat(headerBlockLength), ErrorStatus.InvalidBlockLength);
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
    }
}
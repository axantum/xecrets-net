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

namespace Axantum.AxCrypt.Core
{
    /// <summary>
    /// Enables an single point of interaction for a an AxCrypt encrypted stream with all but the data available
    /// in-memory.
    /// </summary>
    public class AxCryptDocument : IDisposable
    {
        public AxCryptDocument()
        {
        }

        private AxCryptReader _axCryptReader;
        private byte[] _hmac;
        private byte[] _masterKey;

        private IList<HeaderBlock> HeaderBlocks { get; set; }

        /// <summary>
        /// Loads an AxCrypt file from the specified reader.
        /// </summary>
        /// <param name="axCryptReader">The reader.</param>
        public void Load(AxCryptReader axCryptReader)
        {
            _axCryptReader = axCryptReader;
            LoadHeaders();
        }

        private void LoadHeaders()
        {
            _axCryptReader.Read();
            if (_axCryptReader.ItemType != AxCryptItemType.MagicGuid)
            {
                throw new FileFormatException("No magic Guid was found.");
            }
            HeaderBlocks = new List<HeaderBlock>();
            while (_axCryptReader.Read())
            {
                switch (_axCryptReader.ItemType)
                {
                    case AxCryptItemType.None:
                        throw new FileFormatException("Header type is not allowed to be 'none'.");
                    case AxCryptItemType.MagicGuid:
                        throw new FileFormatException("Duplicate magic Guid found.");
                    case AxCryptItemType.HeaderBlock:
                        HeaderBlocks.Add(_axCryptReader.HeaderBlock);
                        break;
                    case AxCryptItemType.Data:
                        ParseHeaders();
                        return;
                    case AxCryptItemType.EndOfStream:
                        throw new FileFormatException("End of stream found too early.");
                    default:
                        throw new FileFormatException("Unknown header type found.");
                }
            }
            throw new FileFormatException("No Data was found.");
        }

        private void ParseHeaders()
        {
            KeyWrap1HeaderBlock keyHeaderBlock = FindHeaderBlock<KeyWrap1HeaderBlock>();
            byte[] wrappedKeyData = keyHeaderBlock.GetKeyData();
            byte[] salt = keyHeaderBlock.GetSalt();
            long iterations = keyHeaderBlock.Iterations();
            byte[] unwrappedKeyData = null;
            using (KeyWrap keyWrap = new KeyWrap(_axCryptReader.Settings.GetDerivedPassphrase(), salt, iterations, KeyWrapMode.AxCrypt))
            {
                unwrappedKeyData = keyWrap.Unwrap(wrappedKeyData);
                if (!KeyWrap.IsKeyUnwrapValid(unwrappedKeyData))
                {
                    return;
                }
            }
            _masterKey = KeyWrap.GetKeyBytes(unwrappedKeyData);

            foreach (HeaderBlock headerBlock in HeaderBlocks)
            {
                switch (headerBlock.HeaderBlockType)
                {
                    case HeaderBlockType.None:
                        break;
                    case HeaderBlockType.Any:
                        break;
                    case HeaderBlockType.Preamble:
                        _hmac = ((PreambleHeaderBlock)headerBlock).GetHmac();
                        break;
                    case HeaderBlockType.Version:
                        VersionHeaderBlock versionHeaderBlock = (VersionHeaderBlock)headerBlock;
                        if (versionHeaderBlock.FileVersionMajor > 3)
                        {
                            throw new FileFormatException("Too new file format.");
                        }
                        break;
                    case HeaderBlockType.KeyWrap1:
                        break;
                    case HeaderBlockType.KeyWrap2:
                        break;
                    case HeaderBlockType.IdTag:
                        break;
                    case HeaderBlockType.Data:
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
                        break;
                }
            }
        }

        private T FindHeaderBlock<T>() where T : class
        {
            foreach (HeaderBlock headerBlock in HeaderBlocks)
            {
                T typedHeaderHeaderBlock = headerBlock as T;
                if (typedHeaderHeaderBlock != null)
                {
                    return typedHeaderHeaderBlock;
                }
            }
            throw new FileFormatException("No header block found.");
        }

        public byte[] GetHmac()
        {
            return (byte[])_hmac.Clone();
        }

        public string FileName
        {
            get
            {
                FileNameInfoHeaderBlock headerBlock = FindHeaderBlock<FileNameInfoHeaderBlock>();

                string fileName = headerBlock.GetFileName(HeaderCrypto);
                return fileName;
            }
        }

        public string UnicodeFileName
        {
            get
            {
                UnicodeFileNameInfoHeaderBlock headerBlock = FindHeaderBlock<UnicodeFileNameInfoHeaderBlock>();

                string fileName = headerBlock.GetFileName(HeaderCrypto);
                return fileName;
            }
        }

        public bool IsCompressed
        {
            get
            {
                CompressionHeaderBlock headerBlock = FindHeaderBlock<CompressionHeaderBlock>();

                return headerBlock.IsCompressed(HeaderCrypto);
            }
        }

        private AesCrypto _headerCrypto;

        private AesCrypto HeaderCrypto
        {
            get
            {
                if (_headerCrypto == null)
                {
                    Subkey headersSubKey = new Subkey(_masterKey, HeaderSubkey.Headers);
                    _headerCrypto = new AesCrypto(headersSubKey.Get());
                }
                return _headerCrypto;
            }
        }

        /// <summary>
        /// Decrypts the encrypted data to the given stream
        /// </summary>
        /// <param name="plainTextStream">The plain text stream.</param>
        public void DecryptTo(Stream plaintextStream)
        {
            while (_axCryptReader.Read())
            {
                switch (_axCryptReader.ItemType)
                {
                    case AxCryptItemType.Data:
                        byte[] dataChunk = _axCryptReader.GetAndOwnDataChunk();
                        plaintextStream.Write(dataChunk, 0, dataChunk.Length);
                        break;
                    case AxCryptItemType.EndOfStream:
                        return;
                    default:
                        throw new FileFormatException("Unexpected header type found.");
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_headerCrypto == null)
            {
                return;
            }

            if (disposing)
            {
                _headerCrypto.Dispose();
                _headerCrypto = null;
            }
        }
    }
}
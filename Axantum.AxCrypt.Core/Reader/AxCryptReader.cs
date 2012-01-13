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
        public static readonly Guid AxCrypt1Guid = new Guid("2e07b9c0-934f-46f1-a015-792ca1d9e821");

        private static readonly byte[] _axCrypt1GuidAsBytes = AxCrypt1Guid.ToByteArray();

        public static byte[] GetAxCrypt1GuidAsBytes()
        {
            return _axCrypt1GuidAsBytes;
        }

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
            switch (ItemType)
            {
                case AxCryptItemType.None:
                    return LookForMagicGuid();
                case AxCryptItemType.MagicGuid:
                    return LookForHeaderBlock();
                case AxCryptItemType.HeaderBlock:
                    return LookForHeaderBlock();
                case AxCryptItemType.EncryptedCompressedData:
                    return LookForData();
                case AxCryptItemType.EncryptedData:
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

        private static bool LookForData()
        {
            throw new NotImplementedException();
        }

        private bool LookForHeaderBlock()
        {
            int blockType = InputStream.ReadByte();
            if (blockType < 0)
            {
                return false;
            }
            HeaderBlockType headerBlockType = (HeaderBlockType)blockType;

            byte[] lengthBytes = new byte[sizeof(Int32)];
            int bytesRead = InputStream.Read(lengthBytes, 0, lengthBytes.Length);
            if (bytesRead != lengthBytes.Length)
            {
                return false;
            }
            Int32 headerBlockLength = BitConverter.ToInt32(lengthBytes, 0);

            byte[] blockData = new byte[headerBlockLength];
            bytesRead = InputStream.Read(blockData, 0, blockData.Length);
            if (bytesRead != blockData.Length)
            {
                return false;
            }

            return ParseHeaderBlock(headerBlockType, blockData);
        }

        private bool ParseHeaderBlock(HeaderBlockType headerBlockType, byte[] blockData)
        {
            switch (headerBlockType)
            {
                case HeaderBlockType.None:
                    break;
                case HeaderBlockType.Any:
                    break;
                case HeaderBlockType.Preamble:
                    break;
                case HeaderBlockType.Version:
                    HeaderBlock = new VersionHeaderBlock(headerBlockType, blockData);
                    return true;
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
            return false;
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
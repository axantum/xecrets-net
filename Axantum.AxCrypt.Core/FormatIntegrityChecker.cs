using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Header;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Reader;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core
{
    public class FormatIntegrityChecker : IDisposable
    {
        private Stream _inputStream;

        private string _fileName;

        public IDictionary<string, string> StatusReport = new Dictionary<string, string>();

        private Stack<ByteBuffer> _pushBack = new Stack<ByteBuffer>();

        public FormatIntegrityChecker(Stream inputStream, string fileName)
        {
            _inputStream = inputStream ?? throw new ArgumentNullException(nameof(inputStream), "inputStream");
            _fileName = fileName ?? throw new ArgumentNullException(nameof(fileName), "fileName");
        }

        public bool Verify()
        {
            byte[] buffer = new byte[_inputStream.Length];
            int bytesRead = _inputStream.Read(buffer, 0, buffer.Length);

            if (bytesRead < 0 && bytesRead > AxCrypt1Guid.Length)
            {
                StatusReport.Add(nameof(AxCryptItemType.EndOfStream), "Not an AxCrypt file, No magic Guid was found.");
                return ShowStatusReport();
            }

            byte[] _axCrypt1GuidBytes = AxCrypt1Guid.GetBytes();
            int i = buffer.Locate(_axCrypt1GuidBytes, 0, AxCrypt1Guid.Length);
            if (i < 0)
            {
                StatusReport.Add(nameof(AxCryptItemType.MagicGuid), "Not found.");
                return ShowStatusReport();
            }
            StatusReport.Add(nameof(AxCryptItemType.MagicGuid), "Ok with the length {0}".InvariantFormat(AxCrypt1Guid.Length));

            int offset = AxCrypt1Guid.Length + i;
            Pushback(buffer, offset, (bytesRead - offset));
            List<HeaderBlock> headerBlocks = new List<HeaderBlock>();
            while (true)
            {
                byte[] lengthBytes = new byte[sizeof(Int32)];

                Read(lengthBytes, 0, lengthBytes.Length);

                Int32 headerBlockLength = BitConverter.ToInt32(lengthBytes, 0) - 5;
                if (headerBlockLength < 0)
                {
                    StatusReport.Add(nameof(AxCryptItemType.HeaderBlock), "This is a Format Error with an Invalid Block Length");
                    return ShowStatusReport();
                }
                int blockType = ReadByte();
                if (blockType > 127)
                {
                    StatusReport.Add(nameof(AxCryptItemType.Undefined), "Invalid block type {0}".InvariantFormat(blockType));
                    return ShowStatusReport();
                }

                HeaderBlockType headerBlockType = (HeaderBlockType)blockType;
                byte[] dataBlock = new byte[headerBlockLength];

                Read(dataBlock, 0, dataBlock.Length);

                HeaderBlock currentHeaderBlock;

                KeyValuePair<string, string> reportHeaderBlock = new KeyValuePair<string, string>();

                switch (headerBlockType)
                {
                    case HeaderBlockType.Preamble:
                        currentHeaderBlock = new PreambleHeaderBlock(dataBlock);
                        break;

                    case HeaderBlockType.Version:
                        VersionHeaderBlock versionHeaderBlock = new VersionHeaderBlock(dataBlock);
                        reportHeaderBlock = new KeyValuePair<string, string>("AxCrypt File Version", versionHeaderBlock.FileVersionMajor + "." + versionHeaderBlock.FileVersionMinor);
                        currentHeaderBlock = versionHeaderBlock;
                        break;

                    case HeaderBlockType.Data:
                        DataHeaderBlock dataHeaderBlock = new DataHeaderBlock(dataBlock);
                        reportHeaderBlock = new KeyValuePair<string, string>("AxCrypt File Text Length", dataHeaderBlock.CipherTextLength.ToString());
                        currentHeaderBlock = dataHeaderBlock;
                        break;

                    case HeaderBlockType.FileInfo:
                        FileInfoEncryptedHeaderBlock fileInfoEncryptedHeaderBlock = new FileInfoEncryptedHeaderBlock(dataBlock);
                        reportHeaderBlock = new KeyValuePair<string, string>("AxCrypt Header", fileInfoEncryptedHeaderBlock.HeaderCrypto.ToString());
                        currentHeaderBlock = fileInfoEncryptedHeaderBlock;

                        break;

                    case HeaderBlockType.PlaintextLengths:
                        V2PlaintextLengthsEncryptedHeaderBlock v2PlaintextLengthsEncryptedHeaderBlock = new V2PlaintextLengthsEncryptedHeaderBlock(dataBlock);
                        reportHeaderBlock = new KeyValuePair<string, string>("", v2PlaintextLengthsEncryptedHeaderBlock.CompressedPlaintextLength.ToString());
                        currentHeaderBlock = v2PlaintextLengthsEncryptedHeaderBlock;
                        break;

                    case HeaderBlockType.EncryptedDataPart:
                        currentHeaderBlock = new EncryptedDataPartBlock(dataBlock);
                        break;

                    case HeaderBlockType.V2Hmac:
                        currentHeaderBlock = new V2HmacHeaderBlock(dataBlock);
                        break;

                    case HeaderBlockType.EncryptionInfo:
                        currentHeaderBlock = new V1EncryptionInfoEncryptedHeaderBlock(dataBlock);
                        break;

                    default:
                        currentHeaderBlock = null;
                        break;
                }

                if (string.IsNullOrEmpty(reportHeaderBlock.Key))
                {
                    StatusReport.Add(reportHeaderBlock);
                }

                if (currentHeaderBlock != null && !StatusReport.ContainsKey(headerBlockType.ToString()))
                {
                    StatusReport.Add(headerBlockType.ToString(), string.Format(@"Ok with the length {0}", headerBlockLength));
                }
            }
        }

        public void Pushback(byte[] buffer, int offset, int length)
        {
            EnsureNotDisposed();
            byte[] pushbackBuffer = new byte[length];
            Array.Copy(buffer, offset, pushbackBuffer, 0, length);
            _pushBack.Push(new ByteBuffer(pushbackBuffer));
        }

        public int ReadByte()
        {
            byte[] lengthBytes = new byte[sizeof(byte)];
            Read(lengthBytes, 0, lengthBytes.Length);
            return lengthBytes[0];
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            EnsureNotDisposed();
            int bytesRead = 0;
            while (count > 0 && _pushBack.Count > 0)
            {
                ByteBuffer byteBuffer = _pushBack.Pop();
                int length = byteBuffer.Read(buffer, offset, count);
                offset += length;
                count -= length;
                bytesRead += length;
                if (byteBuffer.AvailableForRead > 0)
                {
                    _pushBack.Push(byteBuffer);
                }
            }
            bytesRead += _inputStream.Read(buffer, offset, count);
            return bytesRead;
        }

        private bool ShowStatusReport()
        {
            if (StatusReport.Any())
            {
                string template = "Structural integrity check of '{0}'".InvariantFormat(_fileName);
                foreach (var report in StatusReport)
                {
                    template += Environment.NewLine;
                    template += report.Key + ":" + report.Value;
                }

                New<IUIThread>().PostTo(async () => await New<IPopup>().ShowAsync(PopupButtons.Ok, "Warning!", template));
                return true;
            }
            return false;
        }

        private bool _disposed = false;

        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        protected virtual void Dispose(bool disposing)
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
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
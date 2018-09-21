using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Header;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Reader;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.UI;
using Org.BouncyCastle.Utilities.Zlib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core
{
    public class FormatIntergrityChecker : IDisposable
    {
        private Stream _inputStream;

        public IDictionary<string, string> StatusReport = new Dictionary<string, string>();

        private static readonly byte[] _axCrypt1GuidBytes = AxCrypt1Guid.GetBytes();

        private Stack<ByteBuffer> _pushBack = new Stack<ByteBuffer>();

        private bool _disposed = false;

        public FormatIntergrityChecker(Stream inputStream)
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException("inputStream");
            }
            _inputStream = inputStream;
        }

        public async Task<bool> Verify()
        {
            byte[] buffer = new byte[OS.Current.StreamBufferSize];
            int bytesRead = _inputStream.Read(buffer, 0, buffer.Length);

            if (bytesRead < AxCrypt1Guid.Length)
            {
                StatusReport.Add(nameof(AxCryptItemType.EndOfStream), "Not an AxCrypt file, No magic Guid was found.");
                GetStatusReport();
                return false;
            }

            int i = buffer.Locate(_axCrypt1GuidBytes, 0, AxCrypt1Guid.Length);
            if (i < 0)
            {
                StatusReport.Add(nameof(AxCryptItemType.MagicGuid), "No magic Guid was found.");
                GetStatusReport();
                return false;
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
                if (headerBlockLength < 0 || headerBlockLength > 0xfffff)
                {
                    StatusReport.Add(nameof(AxCryptItemType.EndOfStream), "End of File");
                    GetStatusReport();
                    return false;
                }
                int blockType = ReadByte();
                if (blockType > 127)
                {
                    StatusReport.Add(nameof(AxCryptItemType.Undefined), "Invalid block type {0}".InvariantFormat(blockType));
                    GetStatusReport();
                    return false;
                }

                HeaderBlockType headerBlockType = (HeaderBlockType)blockType;
                byte[] dataBlock = new byte[headerBlockLength];

                Read(dataBlock, 0, dataBlock.Length);

                HeaderBlock currentHeaderBlock;
                try
                {
                    switch (headerBlockType)
                    {
                        case HeaderBlockType.Preamble:
                            currentHeaderBlock = new PreambleHeaderBlock(dataBlock);
                            break;
                        case HeaderBlockType.Version:
                            currentHeaderBlock = new VersionHeaderBlock(dataBlock);
                            break;
                        case HeaderBlockType.Data:
                            currentHeaderBlock = new DataHeaderBlock(dataBlock);
                            break;
                        case HeaderBlockType.FileInfo:
                            currentHeaderBlock = new FileInfoEncryptedHeaderBlock(dataBlock);
                            break;
                        case HeaderBlockType.PlaintextLengths:
                            currentHeaderBlock = new V2PlaintextLengthsEncryptedHeaderBlock(dataBlock);
                            break;
                        case HeaderBlockType.EncryptedDataPart:
                            currentHeaderBlock = new EncryptedDataPartBlock(dataBlock);
                            break;
                        case HeaderBlockType.EncryptionInfo:
                            currentHeaderBlock = new V1EncryptionInfoEncryptedHeaderBlock(dataBlock);
                            break;
                        case HeaderBlockType.V2Hmac:
                            currentHeaderBlock = new V2HmacHeaderBlock(dataBlock);
                            break;
                        default:
                            currentHeaderBlock = null;
                            break;
                    }

                    if (currentHeaderBlock != null && !StatusReport.ContainsKey(headerBlockType.ToString()))
                    {
                        StatusReport.Add(headerBlockType.ToString(), string.Format(@"Ok with the length {0}", headerBlockLength));
                    }
                }
                catch (AxCryptException aex)
                {
                    StatusReport.Add(headerBlockType.ToString(), aex.InnerException.Message);
                    GetStatusReport();
                    return false;
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

        private void GetStatusReport()
        {
            if (StatusReport.Any())
            {
                string template = "";
                foreach (var report in StatusReport)
                {
                    template += report.Key + ":" + report.Value;
                    template += Environment.NewLine;
                }

                New<IUIThread>().PostTo(async () => await New<IPopup>().ShowAsync(PopupButtons.Ok, "Warning!", template));
            }
        }

        public void Dispose()
        {
            if (_inputStream != null)
            {
                _inputStream.Dispose();
                _inputStream = null;
            }
            _disposed = true;
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}


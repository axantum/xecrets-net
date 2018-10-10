using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Header;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Reader;
using Axantum.AxCrypt.Core.Runtime;
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
            try
            {
                return VerifyInternalUnsafe();
            }
            catch (Exception ex) when (!(ex is AxCryptException))
            {
                throw new FileOperationException("Format integrity check failed", _fileName, ErrorStatus.Exception, ex);
            }
        }

        private bool VerifyInternalUnsafe()
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
            StatusReport.Add(nameof(AxCryptItemType.MagicGuid), $"Ok, with {AxCrypt1Guid.Length} bytes.");

            int offset = AxCrypt1Guid.Length + i;
            Pushback(buffer, offset, (bytesRead - offset));
            List<HeaderBlock> headerBlocks = new List<HeaderBlock>();
            int totalHeaderBlocks = 0;

            while (true)
            {
                byte[] lengthBytes = new byte[sizeof(Int32)];

                Read(lengthBytes, 0, lengthBytes.Length);

                Int32 headerBlockLength = BitConverter.ToInt32(lengthBytes, 0) - 5;
                if (headerBlockLength < 0)
                {
                    StatusReport.Add(nameof(AxCryptItemType.HeaderBlock), "This is a format error with an Invalid block length or End of File.");
                    StatusReport.Add("Total Header blocks", totalHeaderBlocks.ToString());

                    return ShowStatusReport();
                }
                int blockType = ReadByte();
                if (blockType > 127)
                {
                    StatusReport.Add("Unexpected header block type", blockType.ToString());
                    StatusReport.Add("Total Header blocks", totalHeaderBlocks.ToString());

                    return ShowStatusReport();
                }

                HeaderBlockType headerBlockType = (HeaderBlockType)blockType;
                byte[] dataBlock = new byte[headerBlockLength];

                Read(dataBlock, 0, dataBlock.Length);

                KeyValuePair<string, string> headerBlockStatus = new KeyValuePair<string, string>();

                switch (headerBlockType)
                {
                    case HeaderBlockType.Version:
                        VersionHeaderBlock versionHeaderBlock = new VersionHeaderBlock(dataBlock);
                        headerBlockStatus = new KeyValuePair<string, string>("Encrypted by", $"v{versionHeaderBlock.VersionMajor}.{versionHeaderBlock.FileVersionMajor}.");
                        break;

                    case HeaderBlockType.Data:
                        headerBlockStatus = new KeyValuePair<string, string>("End of header blocks", "***");
                        break;

                    case HeaderBlockType.EncryptedDataPart:
                        headerBlockStatus = new KeyValuePair<string, string>("Encrypted Data size", $"{dataBlock.Length} bytes.");
                        break;

                    default:
                        headerBlockStatus = new KeyValuePair<string, string>(((HeaderBlockType)headerBlockType).ToString(), $"Ok, with {dataBlock.Length} bytes.");
                        break;
                }

                if (!string.IsNullOrEmpty(headerBlockStatus.Key) && !StatusReport.ContainsKey(headerBlockStatus.Key))
                {
                    StatusReport.Add(headerBlockStatus);
                    totalHeaderBlocks++;
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
                template += Environment.NewLine;
                foreach (var report in StatusReport)
                {
                    template += Environment.NewLine;
                    template += report.Key + ": " + report.Value;
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
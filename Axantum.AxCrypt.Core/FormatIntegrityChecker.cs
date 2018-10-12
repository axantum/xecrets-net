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
        private LookAheadStream _inputStream;

        private string _fileName;

        private List<string> _statusReport = new List<string>();

        private bool _isOk = true;

        public FormatIntegrityChecker(Stream inputStream, string fileName)
        {
            _inputStream = new LookAheadStream(inputStream ?? throw new ArgumentNullException(nameof(inputStream), "inputStream"));
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

        private static readonly byte[] _axCrypt1GuidBytes = AxCrypt1Guid.GetBytes();

        private enum AxCryptVersion
        {
            Unknown,
            Version1,
            Version2,
        }

        private bool VerifyInternalUnsafe()
        {
            if (!_inputStream.Locate(_axCrypt1GuidBytes))
            {
                return FailWithStatusReport("Not an AxCrypt file, No magic Guid was found.");
            }

            _statusReport.Add($"{nameof(AxCryptItemType.MagicGuid)} Ok with length {0}".InvariantFormat(AxCrypt1Guid.Length));

            AxCryptVersion version = AxCryptVersion.Unknown;
            ulong encryptedDataLength = 0;
            int dataBlocks = 0;
            while (true)
            {
                byte[] lengthBytes = new byte[sizeof(Int32)];

                if (_inputStream.Read(lengthBytes, 0, lengthBytes.Length) != lengthBytes.Length)
                {
                    return FailWithStatusReport("End of stream reading header block length.");
                }

                int headerBlockLength = BitConverter.ToInt32(lengthBytes, 0) - 5;
                int blockType = _inputStream.ReadByte();
                if (blockType > 127 || blockType < 0)
                {
                    return FailWithStatusReport($"Unexpected header block type {blockType}");
                }

                if (headerBlockLength < 0)
                {
                    return FailWithStatusReport($"Invalid block length {headerBlockLength}.");
                }

                byte[] dataBlock = new byte[headerBlockLength];

                if (_inputStream.Read(dataBlock, 0, headerBlockLength) != dataBlock.Length)
                {
                    return FailWithStatusReport($"End of stream reading block type {blockType}");
                }

                HeaderBlockType headerBlockType = (HeaderBlockType)blockType;
                if (headerBlockType == HeaderBlockType.Data && version == AxCryptVersion.Version1)
                {
                    return ProcessVersion1DataBlock(dataBlock);
                }

                if (headerBlockType != HeaderBlockType.EncryptedDataPart && dataBlocks > 0)
                {
                    _statusReport.Add($"{HeaderBlockType.EncryptedDataPart} Ok with {dataBlocks} blocks and the total length {encryptedDataLength}.");
                    dataBlocks = 0;
                    encryptedDataLength = 0;
                }

                switch (headerBlockType)
                {
                    case HeaderBlockType.Version:
                        VersionHeaderBlock versionHeaderBlock = new VersionHeaderBlock(dataBlock);
                        _statusReport.Add($"AxCrypt version {versionHeaderBlock.VersionMajor}.{versionHeaderBlock.VersionMinor}.{versionHeaderBlock.VersionMinuscule}. File format version {versionHeaderBlock.FileVersionMajor}.{versionHeaderBlock.FileVersionMinor}.");
                        version = versionHeaderBlock.VersionMajor >= 2 ? AxCryptVersion.Version2 : AxCryptVersion.Version1;
                        break;

                    case HeaderBlockType.EncryptedDataPart:
                        switch (version)
                        {
                            case AxCryptVersion.Version2:
                                ++dataBlocks;
                                encryptedDataLength += (uint)dataBlock.Length;
                                break;

                            case AxCryptVersion.Unknown:
                            default:
                                return FailWithStatusReport($"{blockType} found but no {HeaderBlockType.Version} seen.");
                        }
                        break;

                    default:
                        _statusReport.Add($"{headerBlockType} Ok with length {headerBlockLength}");
                        break;
                }

                if (headerBlockType == HeaderBlockType.V2Hmac)
                {
                    return ShowStatusReport();
                }
            }
        }

        private bool FailWithStatusReport(string report)
        {
            _isOk = false;
            _statusReport.Add(report);
            return ShowStatusReport();
        }

        private bool ProcessVersion1DataBlock(byte[] dataBlock)
        {
            long headerBlockLength = BitConverter.ToInt64(dataBlock, 0);
            if (headerBlockLength < 0)
            {
                return FailWithStatusReport($"{HeaderBlockType.Data} found but length is tool large to fit a long: {headerBlockLength}.");
            }
            if (!_inputStream.Skip(headerBlockLength))
            {
                return FailWithStatusReport($"{HeaderBlockType.Data} found but end of file was reached before reading all data: {headerBlockLength}.");
            }
            return ShowStatusReport();
        }

        private bool ShowStatusReport()
        {
            string template = "Structural integrity check of:\n'{0}'\n".InvariantFormat(_fileName);
            foreach (string report in _statusReport)
            {
                template += Environment.NewLine;
                template += report;
            }

            New<IUIThread>().PostTo(async () => await New<IPopup>().ShowAsync(PopupButtons.Ok, "Warning!", template));
            return _isOk;
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
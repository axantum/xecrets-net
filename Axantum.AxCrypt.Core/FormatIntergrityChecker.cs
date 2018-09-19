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
    public class FormatIntergrityChecker
    {
        private Stream _stream;

        public IDictionary<AxCryptItemType, string> StatusReport = new Dictionary<AxCryptItemType, string>();

        public string ErrorMessage { get; set; }

        private static readonly byte[] _axCrypt1GuidBytes = AxCrypt1Guid.GetBytes();

        public FormatIntergrityChecker(Stream stream)
        {
            _stream = stream;
        }

        public async Task<bool> Verify()
        {
            byte[] buffer = new byte[OS.Current.StreamBufferSize];
            int bytesRead = _stream.Read(buffer, 0, buffer.Length);

            if (bytesRead < AxCrypt1Guid.Length)
            {
                StatusReport.Add(AxCryptItemType.EndOfStream, "Not an AxCrypt file, No magic Guid was found.");

            }

            int i = buffer.Locate(_axCrypt1GuidBytes, 0, AxCrypt1Guid.Length);
            if (i < 0)
            {
                StatusReport.Add(AxCryptItemType.MagicGuid, "No magic Guid was found.");
                GetStatusReport();
                return false;
            }
            StatusReport.Add(AxCryptItemType.MagicGuid, "Ok");

            byte[] pushbackBuffer = new byte[length];
            Array.Copy(buffer, offset, pushbackBuffer, 0, length);
            _pushBack.Push(new ByteBuffer(pushbackBuffer));
             int offset = AxCrypt1Guid.Length + 1;

            byte[] lengthBytes = new byte[sizeof(Int32)];
            _stream.Read(lengthBytes, 0, lengthBytes.Length);
            Int32 headerBlockLength = BitConverter.ToInt32(lengthBytes, 0) - 5;

            if (headerBlockLength < 0 || headerBlockLength > 0xfffff)
            {
                StatusReport.Add(AxCryptItemType.HeaderBlock, "Invalid headerBlockLength {0}".InvariantFormat(headerBlockLength));
                GetStatusReport();
                return false;
            }

            int blockType = _stream.ReadByte();
            if (blockType > 127)
            {
                StatusReport.Add(AxCryptItemType.HeaderBlock, "Invalid block type {0}".InvariantFormat(blockType));
                GetStatusReport();
                return false;
            }

            HeaderBlockType headerBlockType = (HeaderBlockType)blockType;

            byte[] dataBlock = new byte[headerBlockLength];
            _stream.Read(dataBlock, 0, dataBlock.Length);

            //ParseHeaderBlock(headerBlockType, dataBlock);

            //DataHeaderBlock dataHeaderBlock = CurrentHeaderBlock as DataHeaderBlock;
            //if (dataHeaderBlock != null)
            //{
            //    CurrentItemType = AxCryptItemType.Data;
            //}

            //int offset = AxCrypt1Guid.Length + 1;
            //_stream.Read(buffer, offset, headerBlockLength);

            GetStatusReport();
            return true;
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

                // System.Console.WriteLine(template);
                New<IUIThread>().PostTo(async () => await New<IPopup>().ShowAsync(PopupButtons.Ok, "Warning!", template));
            }
        }
    }
}


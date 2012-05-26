using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.UI;

namespace Axantum.AxCrypt
{
    public class WorkerArguments
    {
        public WorkerArguments(IRuntimeFileInfo sourceFileInfo, IRuntimeFileInfo destinationFileInfo, AesKey key, ProgressContext progress)
        {
            SourceFileInfo = sourceFileInfo;
            DestinationFileInfo = destinationFileInfo;
            Key = key;
            Progress = progress;
        }

        public IRuntimeFileInfo SourceFileInfo { get; private set; }

        public IRuntimeFileInfo DestinationFileInfo { get; private set; }

        public AesKey Key { get; private set; }

        public ProgressContext Progress { get; private set; }
    }
}
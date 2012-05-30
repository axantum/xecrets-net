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
        public WorkerArguments(IRuntimeFileInfo sourceFileInfo, IRuntimeFileInfo destinationFileInfo, IList<AesKey> keys, ProgressContext progress)
        {
            SourceFileInfo = sourceFileInfo;
            DestinationFileInfo = destinationFileInfo;
            Keys = keys;
            Progress = progress;
        }

        public IRuntimeFileInfo SourceFileInfo { get; private set; }

        public IRuntimeFileInfo DestinationFileInfo { get; private set; }

        public IList<AesKey> Keys { get; private set; }

        public ProgressContext Progress { get; private set; }
    }
}
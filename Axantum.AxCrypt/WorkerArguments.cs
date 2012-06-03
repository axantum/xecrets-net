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
        public WorkerArguments(ProgressContext progress)
        {
            Progress = progress;
        }

        public ProgressContext Progress { get; private set; }

        public object Result { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.UI
{
    public interface IBackgroundWork
    {
        void BackgroundWorkWithProgress(Func<IProgressContext, FileOperationStatus> work, Action<FileOperationStatus> complete);
    }
}
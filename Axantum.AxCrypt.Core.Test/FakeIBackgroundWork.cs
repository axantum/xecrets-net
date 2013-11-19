using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Test
{
    internal class FakeIBackgroundWork : IBackgroundWork
    {
        public void BackgroundWorkWithProgress(Func<IProgressContext, FileOperationStatus> work, Action<FileOperationStatus> complete)
        {
            FileOperationStatus status = work(new ProgressContext());
            complete(status);
        }

        public void WaitForIdle()
        {
        }
    }
}
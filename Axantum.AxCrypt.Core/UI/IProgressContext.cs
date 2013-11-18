using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.UI
{
    public interface IProgressContext
    {
        event EventHandler<ProgressEventArgs> Progressing;

        IProgressContext Progress { get; }

        void RemoveCount(long totalCount, long progressCount);

        void AddTotal(long count);

        void AddCount(long count);

        void NotifyLevelStart();

        void NotifyLevelFinished();

        bool Cancel { get; set; }

        bool AllItemsConfirmed { get; set; }
    }
}